using AutoApiTester.DTOs.SettingJob;
using AutoApiTester.Models;

namespace AutoApiTester.App.Services
{
    public interface IJobApiTestSuiteService
    {
        Task<List<JobApiTestSuiteEntity>> UpsertAsync(List<JobApiTestSuiteDto> dtoList, string userName);
    }
}
