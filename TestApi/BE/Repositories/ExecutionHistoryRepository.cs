using AutoApiTester.App.Repositories;
using AutoApiTester.Data;
using AutoApiTester.Models;
using AutoApiTester.Models.DTOs;
using Microsoft.EntityFrameworkCore;

namespace AutoApiTester.Repositories;

public class ExecutionHistoryRepository : Repository<ExecutionHistoryEntity>, IExecutionHistoryRepository
{
    private readonly ApplicationDbContext _context;

    public ExecutionHistoryRepository(ApplicationDbContext context) : base(context)
    {
        _context = context;
    }

    // ✅ Lấy lịch sử của user
    public async Task<IEnumerable<ExecutionHistoryResponseDto>> GetByUserIdAsync(int userId, int limit = 50)
    {
        return await _context.ExecutionHistories
            .Where(eh => eh.UserId == userId)
            .OrderByDescending(eh => eh.ExecutedAt)
            .Take(limit)
            .Select(eh => new ExecutionHistoryResponseDto
            {
                Id = eh.Id,
                RequestId = eh.RequestId,
                Method = eh.Method,
                Url = eh.Url,
                StatusCode = eh.StatusCode,
                StatusText = eh.StatusText,
                ResponseTime = eh.ResponseTime,
                ExecutedAt = eh.ExecutedAt,
                ErrorMessage = eh.ErrorMessage
            })
            .ToListAsync();
    }

    // ✅ Lấy lịch sử của 1 request
    public async Task<IEnumerable<ExecutionHistoryResponseDto>> GetByRequestIdAsync(int requestId, int limit = 20)
    {
        return await _context.ExecutionHistories
            .Where(eh => eh.RequestId == requestId)
            .OrderByDescending(eh => eh.ExecutedAt)
            .Take(limit)
            .Select(eh => new ExecutionHistoryResponseDto
            {
                Id = eh.Id,
                RequestId = eh.RequestId,
                Method = eh.Method,
                Url = eh.Url,
                StatusCode = eh.StatusCode,
                StatusText = eh.StatusText,
                ResponseTime = eh.ResponseTime,
                ExecutedAt = eh.ExecutedAt,
                ErrorMessage = eh.ErrorMessage
            })
            .ToListAsync();
    }

    // ✅ Lấy chi tiết
    public async Task<ExecutionHistoryDetailDto?> GetDetailAsync(int id, int userId)
    {
        return await _context.ExecutionHistories
            .Where(eh => eh.Id == id && eh.UserId == userId)
            .Select(eh => new ExecutionHistoryDetailDto
            {
                Id = eh.Id,
                UserId = eh.UserId,
                RequestId = eh.RequestId,
                Method = eh.Method,
                Url = eh.Url,
                Headers = eh.Headers,
                QueryParams = eh.QueryParams,
                Body = eh.Body,
                StatusCode = eh.StatusCode,
                StatusText = eh.StatusText,
                ResponseHeaders = eh.ResponseHeaders,
                ResponseBody = eh.ResponseBody,
                ResponseTime = eh.ResponseTime,
                ErrorMessage = eh.ErrorMessage,
                ExecutedAt = eh.ExecutedAt
            })
            .FirstOrDefaultAsync();
    }

    // ✅ Lưu lịch sử mới
    public async Task<ExecutionHistoryResponseDto> CreateAsync(CreateExecutionHistoryDto dto, int userId)
    {
        try
        {
            var history = new ExecutionHistoryEntity
            {
                UserId = userId,
                RequestId = dto.RequestId,
                Method = dto.Method,
                Url = dto.Url,
                Headers = dto.Headers,
                QueryParams = dto.QueryParams,
                Body = dto.Body,
                StatusCode = dto.StatusCode,
                StatusText = dto.StatusText,
                ResponseHeaders = dto.ResponseHeaders,
                ResponseBody = dto.ResponseBody,
                ResponseTime = dto.ResponseTime,
                ErrorMessage = dto.ErrorMessage,
                ExecutedAt = dto.ExecutedAt,
                CreatedAt = DateTime.UtcNow
            };

            await _context.ExecutionHistories.AddAsync(history);
            await _context.SaveChangesAsync();

            return new ExecutionHistoryResponseDto
            {
                Id = history.Id,
                RequestId = history.RequestId,
                Method = history.Method,
                Url = history.Url,
                StatusCode = history.StatusCode,
                StatusText = history.StatusText,
                ResponseTime = history.ResponseTime,
                ExecutedAt = history.ExecutedAt,
                ErrorMessage = history.ErrorMessage
            };
        }catch(Exception ex)
        {
            throw new Exception($"Error creating execution history: {ex.Message}", ex);
        }

    }

    // ✅ Xóa lịch sử cũ (cleanup)
    public async Task<int> DeleteOldHistoriesAsync(int userId, int daysToKeep = 30)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysToKeep);

        var oldHistories = await _context.ExecutionHistories
            .Where(eh => eh.UserId == userId && eh.ExecutedAt < cutoffDate)
            .ToListAsync();

        _context.ExecutionHistories.RemoveRange(oldHistories);
        await _context.SaveChangesAsync();

        return oldHistories.Count;
    }

    // ✅ Lấy lịch sử của user
    public async Task<ExecutionHistoryResponseDto?> GetOneByUserIdAndRequestIdAsync(int userId, int requestId)
    {
        return await _context.ExecutionHistories
            .Where(eh => eh.UserId == userId && eh.RequestId == requestId)
            .OrderByDescending(eh => eh.ExecutedAt)
            .Select(eh => new ExecutionHistoryResponseDto
            {
                Id = eh.Id,
                RequestId = eh.RequestId,
                Method = eh.Method,
                Url = eh.Url,
                StatusCode = eh.StatusCode,
                StatusText = eh.StatusText,
                ResponseTime = eh.ResponseTime,
                ExecutedAt = DateTime.Now,
                ErrorMessage = eh.ErrorMessage
            })
            .FirstOrDefaultAsync(); 
    }



}