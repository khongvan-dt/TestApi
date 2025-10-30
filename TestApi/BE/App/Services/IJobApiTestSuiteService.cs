using AutoApiTester.DTOs.SettingJob;
using AutoApiTester.Models;

namespace AutoApiTester.App.Services
{
    public interface IJobApiTestSuiteService
    {
        Task<JobApiTestSuite> UpsertAsync(JobApiTestSuiteDto dto, string userName);
    }
}
