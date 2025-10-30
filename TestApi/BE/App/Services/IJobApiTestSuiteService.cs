using AutoApiTester.DTOs.SettingJob;
using AutoApiTester.Models;

namespace AutoApiTester.App.Services
{
    public interface IJobApiTestSuiteService
    {
        Task<List<JobApiTestSuite>> UpsertAsync(List<JobApiTestSuiteDto> dtoList, string userName);
    }
}
