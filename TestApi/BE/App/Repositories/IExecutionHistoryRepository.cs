using AutoApiTester.Models;
using AutoApiTester.Models.DTOs;

namespace AutoApiTester.App.Repositories;

public interface IExecutionHistoryRepository : IRepository<ExecutionHistoryEntity>
{
    // Lấy lịch sử của user
    Task<IEnumerable<ExecutionHistoryResponseDto>> GetByUserIdAsync(int userId, int limit = 50);

    // Lấy lịch sử của 1 request
    Task<IEnumerable<ExecutionHistoryResponseDto>> GetByRequestIdAsync(int requestId, int limit = 20);

    // Lấy chi tiết 1 execution
    Task<ExecutionHistoryDetailDto?> GetDetailAsync(int id, int userId);

    // Lưu lịch sử mới
    Task<ExecutionHistoryResponseDto> CreateAsync(CreateExecutionHistoryDto dto, int userId);

    // Xóa lịch sử cũ (để tránh DB quá lớn)
    Task<int> DeleteOldHistoriesAsync(int userId, int daysToKeep = 30);
    Task<ExecutionHistoryResponseDto?> GetOneByUserIdAndRequestIdAsync(int userId, int requestId);
}