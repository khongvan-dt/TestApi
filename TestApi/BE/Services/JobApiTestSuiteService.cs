using AutoApiTester.App.Repositories;
using AutoApiTester.App.Services;
using AutoApiTester.DTOs.SettingJob;
using AutoApiTester.Models;
using System.Threading.Tasks;

namespace AutoApiTester.Services
{
    // Giả định IJobApiTestSuiteService đã được cập nhật với hàm mới.
    public class JobApiTestSuiteService : IJobApiTestSuiteService
    {
        private readonly IJobApiTestSuiteRepository _repository;
        private readonly IRequestBodyRepository _bodyRepository; // Có thể không cần dùng nếu logic RequestBody được xử lý ở tầng cao hơn

        public JobApiTestSuiteService(IJobApiTestSuiteRepository repository, IRequestBodyRepository bodyRepository)
        {
            _repository = repository;
            _bodyRepository = bodyRepository;
        }

        /// <summary>
        /// Tạo mới hoặc cập nhật Job Schedule, các Test Suites và Test Cases con.
        /// </summary>
        /// <param name="jobDto">DTO chứa thông tin Job Schedule và các Test Suites lồng nhau.</param>
        /// <param name="userId">ID của người dùng đang thực hiện thao tác.</param>
        /// <param name="userName">Tên người dùng đang thực hiện thao tác.</param>
        /// <returns>JobScheduleApiTest Entity đã được lưu.</returns>
        public async Task<JobScheduleApiTestEntity> UpsertJobScheduleAsync(
             JobScheduleDto jobDto,
             int userId,
             string userName)
        {
            return await _repository.UpsertJobScheduleAsync(jobDto, userId, userName);
        }
        public async Task<List<JobScheduleApiTestEntity>> GetJobSchedulesByUserIdAsync(int userId)
        {
            return await _repository.GetJobSchedulesByUserIdAsync(userId);
        }

        public async Task<JobScheduleApiTestEntity?> GetJobScheduleDetailAsync(int jobScheduleId, int userId)
        {
            return await _repository.GetJobScheduleDetailAsync(jobScheduleId, userId);
        }
        public async Task<JobScheduleApiTestEntity?> ToggleJobScheduleStatusAsync(int jobScheduleId, int userId)
        {
            return await _repository.ToggleJobScheduleStatusAsync(jobScheduleId, userId);
        }
    }
}