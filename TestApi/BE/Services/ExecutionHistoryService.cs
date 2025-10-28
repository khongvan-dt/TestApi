using AutoApiTester.App.Repositories;
using AutoApiTester.App.Services;
using AutoApiTester.Models.DTOs;

namespace AutoApiTester.Services;

public class ExecutionHistoryService : IExecutionHistoryService
{
    private readonly IExecutionHistoryRepository _repository;

    public ExecutionHistoryService(IExecutionHistoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<ExecutionHistoryResponseDto>> GetUserHistoriesAsync(int userId, int limit = 50)
    {
        return await _repository.GetByUserIdAsync(userId, limit);
    }

    public async Task<IEnumerable<ExecutionHistoryResponseDto>> GetRequestHistoriesAsync(int requestId, int userId)
    {
        return await _repository.GetByRequestIdAsync(requestId);
    }

    public async Task<ExecutionHistoryDetailDto?> GetHistoryDetailAsync(int id, int userId)
    {
        return await _repository.GetDetailAsync(id, userId);
    }

    public async Task<ExecutionHistoryResponseDto> SaveExecutionAsync(CreateExecutionHistoryDto dto)
    {
        return await _repository.CreateAsync(dto);
    }

    public async Task<int> CleanupOldHistoriesAsync(int userId, int daysToKeep = 30)
    {
        return await _repository.DeleteOldHistoriesAsync(userId, daysToKeep);
    }
}