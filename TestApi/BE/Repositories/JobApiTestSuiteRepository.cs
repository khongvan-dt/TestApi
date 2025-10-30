

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

        /// <summary>
        /// Thêm mới hoặc cập nhật 1 JobApiTestSuite (kèm các test case con)
        /// </summary>
        public async Task<JobApiTestSuite> UpsertAsync(JobApiTestSuiteDto dto, string userName)
        {
            JobApiTestSuite entity;

            if (dto.Id == null || dto.Id == 0)
            {
                // INSERT mới
                entity = new JobApiTestSuite
                {
                    Name = dto.Name ?? "Unnamed Suite",
                    Endpoint = dto.Endpoint,
                    Method = dto.Method,
                    Headers =  dto.Headers,
                    DataBase = JsonSerializer.Serialize(dto.DataBase),
                    Description = dto.Description,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userName,
                    IsActive = true
                };

                // Map test cases
                entity.TestCases = dto.TestCases.Select(tc => new JobApiTestCase
                {
                    CaseName = tc.CaseName ?? "Untitled Case",
                    TestData = JsonSerializer.Serialize(tc.TestData),
                    ExpectedStatus = tc.ExpectedStatus
                }).ToList();

                _context.JobApiTestSuites.Add(entity);
            }
            else
            {
                //  UPDATE
                entity = await _context.JobApiTestSuites
                    .Include(s => s.TestCases)
                    .FirstOrDefaultAsync(s => s.Id == dto.Id);

                if (entity == null)
                    throw new Exception($"JobApiTestSuite with Id={dto.Id} not found.");

                entity.Name = dto.Name ?? entity.Name;
                entity.Endpoint = dto.Endpoint;
                entity.Method = dto.Method;
                entity.Headers = JsonSerializer.Serialize(dto.Headers);
                entity.DataBase = JsonSerializer.Serialize(dto.DataBase);
                entity.Description = dto.Description;
                entity.UpdatedAt = DateTime.UtcNow;
                entity.UpdatedBy = userName;

                // Xử lý test case (insert/update)
                foreach (var tcDto in dto.TestCases)
                {
                    if (tcDto.Id == null || tcDto.Id == 0)
                    {
                        // Thêm mới
                        entity.TestCases.Add(new JobApiTestCase
                        {
                            CaseName = tcDto.CaseName ?? "Untitled Case",
                            TestData = JsonSerializer.Serialize(tcDto.TestData),
                            ExpectedStatus = tcDto.ExpectedStatus
                        });
                    }
                    else
                    {
                        // Cập nhật
                        var existing = entity.TestCases.FirstOrDefault(t => t.Id == tcDto.Id);
                        if (existing != null)
                        {
                            existing.CaseName = tcDto.CaseName ?? existing.CaseName;
                            existing.TestData = JsonSerializer.Serialize(tcDto.TestData);
                            existing.ExpectedStatus = tcDto.ExpectedStatus;
                        }
                    }
                }
            }

            await _context.SaveChangesAsync();
            return entity;
        }
    }
}
