using AutoApiTester.Models.DTOs;

namespace AutoApiTester.App.Services;

public interface IExecutionHistoryService
{
    Task<IEnumerable<ExecutionHistoryResponseDto>> GetUserHistoriesAsync(int userId, int limit = 50);
    Task<IEnumerable<ExecutionHistoryResponseDto>> GetRequestHistoriesAsync(int requestId, int userId);
    Task<ExecutionHistoryDetailDto?> GetHistoryDetailAsync(int id, int userId);
    Task<ExecutionHistoryResponseDto> SaveExecutionAsync(CreateExecutionHistoryDto dto);
    Task<int> CleanupOldHistoriesAsync(int userId, int daysToKeep = 30);
}