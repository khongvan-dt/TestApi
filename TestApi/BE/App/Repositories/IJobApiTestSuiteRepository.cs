using AutoApiTester.DTOs.SettingJob;
using AutoApiTester.Models;

namespace AutoApiTester.App.Repositories
{
    public interface IJobApiTestSuiteRepository
    {
        Task<List<JobApiTestSuite>> UpsertAsync(List<JobApiTestSuiteDto> dtoList, string userName);
    }
}
