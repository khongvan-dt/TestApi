using AutoApiTester.App.Repositories;
using AutoApiTester.App.Services;
using AutoApiTester.Models.DTOs;
using Humanizer;

namespace AutoApiTester.Services;

public class DataExportService : IDataExportService
{
    private readonly IDataExportRepository _repository;

    public DataExportService(IDataExportRepository repository)
    {
        _repository = repository;
    }

     public async Task<object> GetAllDataAsync()
    {
        return await _repository.GetAllDataAsync();
    }

     public async Task<UserDataExportDto> GetUserDataAsync(int userId)
    {
        return await _repository.GetUserDataAsync(userId);
    }
    public async Task<ImportResultDto> ImportUserDataAsync(int userId, UserDataExportDto importData)
    {
        return await _repository.ImportUserDataAsync(userId, importData);
    }
    public async Task<List<SaveRequestResultDto>> SaveRequestsAsync(int userId, List<SaveRequestDto> dtos)
    {
        return await _repository.SaveRequestsAsync(userId, dtos);
    }
    public async Task<SaveRequestResultDto> DeleteRequestAsync(int userId, int requestId)
    {
        return await _repository.DeleteRequestAsync(userId, requestId);
    }
}