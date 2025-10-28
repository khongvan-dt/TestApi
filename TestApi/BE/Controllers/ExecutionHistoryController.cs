using AutoApiTester.App.Services;
using AutoApiTester.Models.DTOs;
using Azure.Core;
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
    /// L?u l?ch s? execution m?i (g?i sau khi execute API)
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<ExecutionHistoryResponseDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> SaveExecution([FromBody] CreateExecutionHistoryDto dto)
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));
            }

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid user ID"));
            }

 
            var history = await _service.SaveExecutionAsync(dto,userId);

            return Ok(
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
    /// Lấy bản ghi execution mới nhất của user theo requestId
    /// </summary>
    [HttpPost("getOneByUserIdAndRequestId")]
    public async Task<IActionResult> GetOneByUserIdAndRequestId([FromBody] int requestId )
    {
        try
        {
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid authentication token"));
            }

            var data = await _service.GetOneByUserIdAndRequestIdAsync(userId.Value, requestId);

            if (data == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("Execution history not found"));
            }

            return Ok(ApiResponse<ExecutionHistoryResponseDto>.SuccessResponse(data, "Execution history retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching execution history");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while fetching execution history"));
        }
    }

}