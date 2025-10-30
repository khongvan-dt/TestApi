using AutoApiTester.DTOs.SettingJob;
using AutoApiTester.Models;

namespace AutoApiTester.App.Repositories
{
    public interface IJobApiTestSuiteRepository
    {
        Task<JobApiTestSuite> UpsertAsync(JobApiTestSuiteDto dto, string userName);
    }
}
