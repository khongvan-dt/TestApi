using AutoApiTester.App.Services;
using AutoApiTester.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AutoApiTester.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ExecutionHistoryController : ControllerBase
{
    private readonly IExecutionHistoryService _service;
    private readonly ILogger<ExecutionHistoryController> _logger;

    public ExecutionHistoryController(
        IExecutionHistoryService service,
        ILogger<ExecutionHistoryController> logger)
    {
        _service = service;
        _logger = logger;
    }

    // ? Helper: Get UserId from token
    private int? GetUserId()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim)) return null;

            return int.TryParse(userIdClaim, out var userId) ? userId : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user ID from token");
            return null;
        }
    }

    /// <summary>
    /// L?y l?ch s? th?c thi c?a user hi?n t?i
    /// </summary>
    /// <param name="limit">S? l??ng records (default: 50, max: 100)</param>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ExecutionHistoryResponseDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMyHistories([FromQuery] int limit = 50)
    {
        try
        {
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid token"));
            }

            // Limit maximum records
            limit = Math.Min(limit, 100);

            var histories = await _service.GetUserHistoriesAsync(userId.Value, limit);

            return Ok(ApiResponse<IEnumerable<ExecutionHistoryResponseDto>>.SuccessResponse(
                histories,
                $"Retrieved {((ICollection<ExecutionHistoryResponseDto>)histories).Count} execution histories"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting execution histories");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred"));
        }
    }

    /// <summary>
    /// L?y chi ti?t 1 execution history
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ExecutionHistoryDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetHistoryDetail(int id)
    {
        try
        {
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid token"));
            }

            var history = await _service.GetHistoryDetailAsync(id, userId.Value);
            if (history == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("Execution history not found"));
            }

            return Ok(ApiResponse<ExecutionHistoryDetailDto>.SuccessResponse(history));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting execution history detail {Id}", id);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred"));
        }
    }

    /// <summary>
    /// L?y l?ch s? c?a 1 request c? th?
    /// </summary>
    [HttpGet("request/{requestId}")]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ExecutionHistoryResponseDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRequestHistories(int requestId)
    {
        try
        {
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid token"));
            }

            var histories = await _service.GetRequestHistoriesAsync(requestId, userId.Value);

            return Ok(ApiResponse<IEnumerable<ExecutionHistoryResponseDto>>.SuccessResponse(
                histories,
                $"Retrieved {((ICollection<ExecutionHistoryResponseDto>)histories).Count} histories for this request"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting request histories for {RequestId}", requestId);
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred"));
        }
    }

    /// <summary>
    /// L?u l?ch s? execution m?i (g?i sau khi execute API)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ExecutionHistoryResponseDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> SaveExecution([FromBody] CreateExecutionHistoryDto dto)
    {
        try
        {
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid token"));
            }

            // Override UserId t? token
            dto.UserId = userId.Value;
            dto.ExecutedAt = DateTime.UtcNow;

            var history = await _service.SaveExecutionAsync(dto);

            return CreatedAtAction(
                nameof(GetHistoryDetail),
                new { id = history.Id },
                ApiResponse<ExecutionHistoryResponseDto>.SuccessResponse(history, "Execution history saved")
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving execution history");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred"));
        }
    }

    /// <summary>
    /// Xóa l?ch s? c? (cleanup)
    /// </summary>
    [HttpDelete("cleanup")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> CleanupOldHistories([FromQuery] int daysToKeep = 30)
    {
        try
        {
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid token"));
            }

            // Limit between 1-365 days
            daysToKeep = Math.Max(1, Math.Min(daysToKeep, 365));

            var deletedCount = await _service.CleanupOldHistoriesAsync(userId.Value, daysToKeep);

            return Ok(ApiResponse<object>.SuccessResponse(
                new { deletedCount, daysToKeep },
                $"Deleted {deletedCount} old execution histories"
            ));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cleaning up execution histories");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred"));
        }
    }

    /// <summary>
    /// L?y th?ng kê execution histories
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatistics([FromQuery] int days = 7)
    {
        try
        {
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid token"));
            }

            // Limit between 1-90 days
            days = Math.Max(1, Math.Min(days, 90));

            var histories = await _service.GetUserHistoriesAsync(userId.Value, 1000);
            var cutoffDate = DateTime.UtcNow.AddDays(-days);

            var recentHistories = histories.Where(h => h.ExecutedAt >= cutoffDate).ToList();

            var stats = new
            {
                totalExecutions = recentHistories.Count,
                successfulExecutions = recentHistories.Count(h => h.StatusCode >= 200 && h.StatusCode < 300),
                failedExecutions = recentHistories.Count(h => h.StatusCode >= 400 || h.StatusCode == null),
                averageResponseTime = recentHistories.Any(h => h.ResponseTime.HasValue)
                    ? (int)recentHistories.Where(h => h.ResponseTime.HasValue).Average(h => h.ResponseTime!.Value)
                    : 0,
                mostUsedMethods = recentHistories
                    .GroupBy(h => h.Method)
                    .Select(g => new { method = g.Key, count = g.Count() })
                    .OrderByDescending(x => x.count)
                    .Take(5)
                    .ToList(),
                statusCodeDistribution = recentHistories
                    .Where(h => h.StatusCode.HasValue)
                    .GroupBy(h => h.StatusCode!.Value / 100 * 100) // Group by 200, 300, 400, 500
                    .Select(g => new { statusRange = $"{g.Key}s", count = g.Count() })
                    .OrderBy(x => x.statusRange)
                    .ToList(),
                period = $"Last {days} days"
            };

            return Ok(ApiResponse<object>.SuccessResponse(stats));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting execution statistics");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred"));
        }
    }
}