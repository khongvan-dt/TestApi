using AutoApiTester.App.Repositories;
using AutoApiTester.App.Services;
using AutoApiTester.DTOs.SettingJob;
using AutoApiTester.Models;
 
namespace AutoApiTester.Services
{
    public class JobApiTestSuiteService: IJobApiTestSuiteService
    {
        private readonly IJobApiTestSuiteRepository _repository;

        public JobApiTestSuiteService(IJobApiTestSuiteRepository repository)
        {
            _repository = repository;
        }
        public async Task<JobApiTestSuite> UpsertAsync(JobApiTestSuiteDto dto, string userName)
        {
            return await _repository.UpsertAsync(dto, userName);

        }


    }
}
