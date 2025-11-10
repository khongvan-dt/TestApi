using AutoApiTester.DTOs.SettingJob;
using AutoApiTester.Models;

namespace AutoApiTester.App.Services
{
    public interface IJobApiTestSuiteService
    {
        Task<JobScheduleApiTestEntity> UpsertJobScheduleAsync(
             JobScheduleDto jobDto,
             int userId,
             string userName);
        Task<List<JobScheduleApiTestEntity>> GetJobSchedulesByUserIdAsync(int userId);

        // Lấy chi tiết 1 Job Schedule (bao gồm TestSuite và TestCase)
        Task<JobScheduleApiTestEntity?> GetJobScheduleDetailAsync(int jobScheduleId, int userId);
        Task<JobScheduleApiTestEntity?> ToggleJobScheduleStatusAsync(int jobScheduleId, int userId);
    }
}
