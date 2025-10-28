using AutoApiTester.Models.DTOs;

namespace AutoApiTester.App.Services;

public interface IDataExportService
{
    Task<object> GetAllDataAsync();
    Task<UserDataExportDto> GetUserDataAsync(int userId);
    Task<ImportResultDto> ImportUserDataAsync(int userId, UserDataExportDto importData);
}
