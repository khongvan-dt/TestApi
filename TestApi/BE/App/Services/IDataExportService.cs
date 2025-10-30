using AutoApiTester.Models.DTOs;

namespace AutoApiTester.App.Services;

public interface IDataExportService
{
    Task<object> GetAllDataAsync();
    Task<UserDataExportDto> GetUserDataAsync(int userId);
    Task<ImportResultDto> ImportUserDataAsync(int userId, UserDataExportDto importData);
    Task<List<SaveRequestResultDto>> SaveRequestsAsync(int userId, List<SaveRequestDto> dtos);
    Task<SaveRequestResultDto> DeleteRequestAsync(int userId, int requestId);
}
