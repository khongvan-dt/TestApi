using AutoApiTester.App.Repositories;
using AutoApiTester.Data;
using AutoApiTester.DTOs.SettingJob;
using AutoApiTester.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AutoApiTester.Repositories
{
    public class JobApiTestSuiteRepository : IJobApiTestSuiteRepository
    {
        private readonly ApplicationDbContext _context;

        public JobApiTestSuiteRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- HÀM NGHIỆP VỤ CHÍNH ---

        public async Task<JobScheduleApiTestEntity> UpsertJobScheduleAsync(
           JobScheduleDto jobDto,
           int userId,
           string userName)
        {
            // Tìm Job hiện tại, bao gồm các Test Suite và Test Case con
            var job = await _context.JobScheduleApiTests
                .Include(j => j.JobApiTestSuites).ThenInclude(s => s.TestCases)
                .FirstOrDefaultAsync(j => j.Id == jobDto.Id);

            bool isNewJob = job == null;

            if (isNewJob)
            {
                // 1. TẠO MỚI Job Schedule
                job = new JobScheduleApiTestEntity
                {
                    UserId = userId,
                    Name = jobDto.Name,
                    Description = jobDto.Description,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    JobApiTestSuites = new List<JobApiTestSuiteEntity>()
                };
                _context.JobScheduleApiTests.Add(job);
            }

            // 2. CẬP NHẬT thông tin Job Schedule
            job.Name = jobDto.Name;
            job.Description = jobDto.Description;
            job.UpdatedAt = isNewJob ? null : DateTime.UtcNow;

            // Cấu hình Lịch chạy (Schedule)
            job.ScheduleType = jobDto.ScheduleType;
            job.RunAtTime = jobDto.ScheduleType == "daily" ?
                            ParseDailyTime(jobDto.DailyTime) : null;
            job.IntervalMinutes = jobDto.ScheduleType == "interval" ?
                                  CalculateIntervalMinutes(jobDto.IntervalValue, jobDto.IntervalUnit!) : null;

            // Xử lý các Test Suites (Insert/Update/Delete)
            HandleTestSuites(job, jobDto.TestSuites, userName);

            // 3. LƯU TẤT CẢ thay đổi (bao gồm Job mẹ và các Job con)
            await _context.SaveChangesAsync();
            return job;
        }

        // --- HÀM HỖ TRỢ NỘI BỘ ---

        private void HandleTestSuites(
            JobScheduleApiTestEntity job,
            List<JobApiTestSuiteDto> dtos,
            string userName)
        {
            // Lấy ID của các Suite cần giữ lại
            var suiteIdsToKeep = dtos.Where(s => s.Id > 0).Select(s => s.Id).Cast<int>().ToList();

            // Xóa các Suite bị loại bỏ (orphans)
            var suitesToRemove = job.JobApiTestSuites.Where(s => !suiteIdsToKeep.Contains(s.Id)).ToList();
            foreach (var suite in suitesToRemove)
            {
                _context.JobApiTestSuites.Remove(suite); // EF Core sẽ xóa Test Cases liên quan
            }

            // Upsert (Insert/Update) các Test Suites còn lại
            foreach (var suiteDto in dtos)
            {
                var suiteEntity = job.JobApiTestSuites.FirstOrDefault(s => s.Id == suiteDto.Id);

                if (suiteEntity == null) // INSERT
                {
                    suiteEntity = new JobApiTestSuiteEntity
                    {
                        Endpoint = suiteDto.Endpoint,
                        Method = suiteDto.Method,
                        Headers = Serialize(suiteDto.Headers),
                        DataBaseTest = Serialize(suiteDto.DataBase),
                        CaseTest = suiteDto.Description ?? "Test Setup",
                        Description = suiteDto.Description,
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = userName,
                        IsActive = true
                    };
                    // Thêm Test Case con (Chỉ Insert)
                    suiteEntity.TestCases = suiteDto.TestCases?.Select(tcDto => new JobApiTestCaseEntity
                    {
                        CaseName = tcDto.CaseName ?? "Untitled Case",
                        TestData = Serialize(tcDto.TestData),
                        ExpectedStatus = tcDto.ExpectedStatus
                    }).ToList() ?? new List<JobApiTestCaseEntity>();

                    job.JobApiTestSuites.Add(suiteEntity);
                }
                else // UPDATE
                {
                    suiteEntity.Endpoint = suiteDto.Endpoint;
                    suiteEntity.Method = suiteDto.Method;
                    suiteEntity.Headers = Serialize(suiteDto.Headers);
                    suiteEntity.DataBaseTest = Serialize(suiteDto.DataBase);
                    suiteEntity.CaseTest = suiteDto.Description ?? suiteEntity.CaseTest;
                    suiteEntity.Description = suiteDto.Description;
                    suiteEntity.UpdatedAt = DateTime.UtcNow;
                    suiteEntity.UpdatedBy = userName;

                    UpdateTestCases(suiteEntity, suiteDto.TestCases);
                }
            }
        }

        private void UpdateTestCases(JobApiTestSuiteEntity suiteEntity, List<JobApiTestCaseDto> testCaseDtos)
        {
            var tcIdsToKeep = testCaseDtos.Where(tc => tc.Id > 0).Select(tc => tc.Id).Cast<int>().ToList();

            // Xóa Test Cases bị loại bỏ
            var tcsToRemove = suiteEntity.TestCases.Where(tc => !tcIdsToKeep.Contains(tc.Id)).ToList();
            foreach (var tc in tcsToRemove)
            {
                suiteEntity.TestCases.Remove(tc);
            }

            // Upsert Test Cases còn lại
            foreach (var tcDto in testCaseDtos)
            {
                var existing = suiteEntity.TestCases.FirstOrDefault(t => t.Id == tcDto.Id);
                if (existing == null) // INSERT
                {
                    suiteEntity.TestCases.Add(new JobApiTestCaseEntity
                    {
                        CaseName = tcDto.CaseName ?? "Untitled Case",
                        TestData = Serialize(tcDto.TestData),
                        ExpectedStatus = tcDto.ExpectedStatus
                    });
                }
                else // UPDATE
                {
                    existing.CaseName = tcDto.CaseName ?? existing.CaseName;
                    existing.TestData = Serialize(tcDto.TestData);
                    existing.ExpectedStatus = tcDto.ExpectedStatus;
                }
            }
        }

        private string? Serialize(object? data)
        {
            if (data == null) return null;
            try
            {
                return JsonSerializer.Serialize(data);
            }
            catch { return null; }
        }

        private TimeSpan? ParseDailyTime(string? dailyTime)
        {
            if (string.IsNullOrEmpty(dailyTime)) return null;

            if (TimeSpan.TryParse(dailyTime, out var result))
            {
                return result;
            }
            return null;
        }

        private int? CalculateIntervalMinutes(int? value, string unit)
        {
            if (value.HasValue && value.Value > 0 && unit == "hours")
            {
                return value.Value * 60;
            }
            return value;
        }


        /// <summary>
        /// Lấy danh sách tất cả Job Schedule của User (không include con)
        /// </summary>
        public async Task<List<JobScheduleApiTestEntity>> GetJobSchedulesByUserIdAsync(int userId)
        {
            return await _context.JobScheduleApiTests
                .Where(j => j.UserId == userId)
                .OrderByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        /// <summary>
        /// Lấy chi tiết đầy đủ 1 Job Schedule (bao gồm TestSuite và TestCase con)
        /// </summary>
        public async Task<JobScheduleApiTestEntity?> GetJobScheduleDetailAsync(int jobScheduleId, int userId)
        {
            return await _context.JobScheduleApiTests
                .Include(j => j.JobApiTestSuites)           // Load TestSuites
                    .ThenInclude(s => s.TestCases)          // Load TestCases của mỗi Suite
                .Where(j => j.Id == jobScheduleId && j.UserId == userId)  // Bảo mật: chỉ lấy Job của User
                .FirstOrDefaultAsync();
        }
        /// <summary>
        /// Bật/Tắt trạng thái Job Schedule
        /// </summary>
        public async Task<JobScheduleApiTestEntity?> ToggleJobScheduleStatusAsync(int jobScheduleId, int userId)
        {
            // Tìm Job Schedule thuộc về User
            var jobSchedule = await _context.JobScheduleApiTests
                .Where(j => j.Id == jobScheduleId && j.UserId == userId)
                .FirstOrDefaultAsync();

            if (jobSchedule == null)
                return null;

            // Toggle trạng thái
            jobSchedule.IsActive = !jobSchedule.IsActive;
            jobSchedule.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return jobSchedule;
        }

    }
}