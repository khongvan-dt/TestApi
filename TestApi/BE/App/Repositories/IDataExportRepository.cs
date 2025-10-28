using AutoApiTester.Models.DTOs;

namespace AutoApiTester.App.Repositories;

public interface IDataExportRepository
{
    Task<object> GetAllDataAsync();
    Task<UserDataExportDto> GetUserDataAsync(int userId);
    Task<ImportResultDto> ImportUserDataAsync(int userId, UserDataExportDto importData);
    Task<SaveRequestResultDto> SaveRequestAsync(int userId, SaveRequestDto dto);

}