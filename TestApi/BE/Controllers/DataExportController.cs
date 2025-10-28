using AutoApiTester.App.Services;
using AutoApiTester.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AutoApiTester.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DataExportController : ControllerBase
{
    private readonly IDataExportService _service;
    private readonly ILogger<DataExportController> _logger;

    public DataExportController(IDataExportService service, ILogger<DataExportController> logger)
    {
        _service = service;
        _logger = logger;
    }

    /// <summary>
    /// Get current user ID from JWT token
    /// </summary>
    private int? GetUserId()
    {
        try
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                return null;

            if (int.TryParse(userIdClaim, out var userId))
                return userId;

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user ID from token");
            return null;
        }
    }

    /// <summary>
    /// Get all data for current authenticated user
    /// </summary>
    [HttpGet("my-data")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<UserDataExportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetMyData()
    {
        try
        {
            var userId = GetUserId();

            if (!userId.HasValue)
            {
                _logger.LogWarning("Invalid or missing user ID in token");
                return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid authentication token"));
            }

            _logger.LogInformation("Fetching data for user: {UserId}", userId.Value);

            var data = await _service.GetUserDataAsync(userId.Value);

            if (data == null || data.User == null)
            {
                _logger.LogWarning("No data found for user: {UserId}", userId.Value);
                return NotFound(ApiResponse<object>.ErrorResponse("User data not found"));
            }

            _logger.LogInformation("Successfully fetched data for user: {UserId}", userId.Value);

            return Ok(ApiResponse<UserDataExportDto>.SuccessResponse(data, "Data retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user data");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while fetching your data"));
        }
    }

    /// <summary>
    /// Get all data from database (Admin only - TODO: Add role check)
    /// </summary>
    [HttpGet("all")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllData()
    {
        try
        {
            var userId = GetUserId();

            if (!userId.HasValue)
            {
                _logger.LogWarning("Unauthorized attempt to access all data");
                return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid authentication token"));
            }

            _logger.LogInformation("Admin user {UserId} fetching all data", userId.Value);

            var data = await _service.GetAllDataAsync();

            _logger.LogInformation("Successfully fetched all data");

            return Ok(ApiResponse<object>.SuccessResponse(data, "All data retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching all data");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while fetching data"));
        }
    }

    /// <summary>
    /// Get data summary for current user
    /// </summary>
    [HttpGet("summary")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<DataSummaryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetSummary()
    {
        try
        {
            var userId = GetUserId();

            if (!userId.HasValue)
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse("Invalid authentication token"));
            }

            var data = await _service.GetUserDataAsync(userId.Value);

            if (data == null)
            {
                return NotFound(ApiResponse<object>.ErrorResponse("User data not found"));
            }

            return Ok(ApiResponse<DataSummaryDto>.SuccessResponse(data.Summary, "Summary retrieved successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user summary");
            return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while fetching summary"));
        }
    }

    /// <summary>
    /// Import user data (collections, requests, params, headers, body)
    /// </summary>
    [HttpPost("import")]
    [Authorize]
    public async Task<IActionResult> ImportUserData([FromBody] UserDataExportDto importData)
    {
        // 1️ Validate authentication
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));
        }

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Invalid user ID"));
        }

        // 2️ Validate input
        if (importData == null)
        {
            return BadRequest(ApiResponse<ImportResultDto>.ErrorResponse("Import data is required"));
        }

        if (importData.Collections == null || !importData.Collections.Any())
        {
            return BadRequest(ApiResponse<ImportResultDto>.ErrorResponse("No collections to import"));
        }

        // 3️ Call service
        try
        {
            var result = await _service.ImportUserDataAsync(userId, importData);

            if (!result.Success)
            {
                return BadRequest(ApiResponse<ImportResultDto>.ErrorResponse(
                    result.ErrorMessage ?? "Import failed"
                ));
            }

            //  Success message với thống kê
            var successMessage = $"Successfully imported {result.ImportedCollections} new collection(s)";

            if (result.UpdatedCollections > 0)
            {
                successMessage += $", updated {result.UpdatedCollections} existing collection(s)";
            }

            successMessage += $" with {result.ImportedRequests + result.UpdatedRequests} total request(s)";

            return Ok(ApiResponse<ImportResultDto>.SuccessResponse(result, successMessage));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing data for user {UserId}", userId);
            return StatusCode(500, ApiResponse<ImportResultDto>.ErrorResponse(
                "Failed to import data. Please check your data format and try again."
            ));
        }
    }
}