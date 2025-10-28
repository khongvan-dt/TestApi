using AutoApiTester.App.Repositories;
using AutoApiTester.App.Services;
using AutoApiTester.Models.DTOs;

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
    public async Task<SaveRequestResultDto> SaveRequestAsync(int userId, SaveRequestDto dto)
    {
        return await _repository.SaveRequestAsync(userId, dto);
    }
}