using AutoApiTester.App.Services;
using AutoApiTester.DTOs.SQLDto;
using AutoApiTester.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AutoApiTester.App.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SQLQueryController : ControllerBase
    {
        private readonly ISQLConnectionDBService _service;
        private readonly ILogger<SQLQueryController> _logger;

        public SQLQueryController(ISQLConnectionDBService service, ILogger<SQLQueryController> logger)
        {
            _service = service;
            _logger = logger;
        }
        [HttpPost("execute")]
        public async Task<IActionResult> ExecuteQuery([FromBody] SQLQueryRequestDto request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var username = User.FindFirst(ClaimTypes.Name)?.Value;

                _logger.LogInformation(
                    "User {Username} (ID: {UserId}) executing query. Query length: {QueryLength} chars",
                    username, userId, request.Query?.Length ?? 0);

                if (!int.TryParse(userId, out var userIdInt))
                {
                    return Unauthorized(new SQLQueryResponse
                    {
                        Success = false,
                        Message = "Invalid UserId"
                    });
                }

                if (string.IsNullOrWhiteSpace(request.ConnectionString))
                {
                    return BadRequest(new SQLQueryResponse
                    {
                        Success = false,
                        Message = "Connection string is required"
                    });
                }

                if (string.IsNullOrWhiteSpace(request.Query))
                {
                    return BadRequest(new SQLQueryResponse
                    {
                        Success = false,
                        Message = "Query is required"
                    });
                }

                if (ContainsDangerousKeywords(request.Query))
                {
                    _logger.LogWarning(
                        "User {Username} attempted to execute dangerous query: {Query}",
                        username, request.Query);

                    return BadRequest(new SQLQueryResponse
                    {
                        Success = false,
                        Message = "Query contains dangerous keywords (DROP, TRUNCATE, ALTER). For safety, these operations are not allowed."
                    });
                }

                var result = await _service.ExecuteQueryAsync(
                     request.ConnectionString,
                     request.Query,
                     userIdInt,
                     request.Timeout ?? 30);                                


                _logger.LogInformation(
                    "Query execution completed. Success: {Success}, Execution time: {ExecutionTime}ms",
                    result.Success, result.ExecutionTimeMs);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ExecuteQuery endpoint");

                return StatusCode(500, new SQLQueryResponse
                {
                    Success = false,
                    Message = $"Internal server error: {ex.Message}"
                });
            }
        }

        private bool ContainsDangerousKeywords(string query)
        {
            var dangerousKeywords = new[]
            {
                "DROP ", "TRUNCATE ", "ALTER ", "EXEC ", "EXECUTE ",
                "xp_", "sp_", "SHUTDOWN", "DELETE FROM sys.", "DELETE FROM information_schema"
            };

            var upperQuery = query.ToUpper();
            return dangerousKeywords.Any(keyword => upperQuery.Contains(keyword.ToUpper()));
        }
    }
}