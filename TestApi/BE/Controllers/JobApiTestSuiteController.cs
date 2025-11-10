using AutoApiTester.App.Repositories;
using AutoApiTester.App.Services;
using AutoApiTester.DTOs.SettingJob;
using AutoApiTester.Models;
using AutoApiTester.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Security.Claims;

namespace AutoApiTester.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class JobApiTestSuiteController : ControllerBase
    {
        private readonly IJobApiTestSuiteService _service;
        private readonly ILogger<JobApiTestSuiteController> _logger;

        public JobApiTestSuiteController(
            IJobApiTestSuiteService service,
            ILogger<JobApiTestSuiteController> logger)
        {
            _service = service;
            _logger = logger;
        }

        /// <summary>
        /// Lấy UserId hiện tại từ JWT Token
        /// </summary>
        private int? GetUserId()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userIdClaim))
                    return null;

                return int.TryParse(userIdClaim, out var userId) ? userId : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user ID from token");
                return null;
            }
        }

        [HttpPost("upsert-schedule")] // Đổi endpoint để phản ánh chức năng
        [Authorize]
         public async Task<ActionResult<JobScheduleApiTestEntity>> UpsertJobSchedule([FromBody] JobScheduleDto jobDto)
        {
            if (jobDto == null)
                return BadRequest(new { message = "Dữ liệu cấu hình Job không hợp lệ." });

            // Lấy User ID và User Name
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdString, out int userId))
            {
                // Nếu không lấy được ID, trả về lỗi 401 hoặc 403
                return Unauthorized(new { message = "Không xác định được người dùng." });
            }
            var userName = User?.Identity?.Name ?? "system";

            try
            {
                 var result = await _service.UpsertJobScheduleAsync(jobDto, userId, userName);

                 return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi upsert Job Schedule cho User ID {UserId}", userId);
                return StatusCode(500, new { message = $"Có lỗi xảy ra khi lưu Job Schedule: {ex.Message}" });
            }
        }



        /// <summary>
        /// Lấy danh sách tất cả Job Schedule của User hiện tại
        /// </summary>
        [HttpGet("list")]
        [Authorize]
        public async Task<ActionResult<List<JobScheduleApiTestEntity>>> GetJobSchedules()
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized(new { message = "Không xác định được người dùng." });

            try
            {
                var jobSchedules = await _service.GetJobSchedulesByUserIdAsync(userId.Value);
                return Ok(jobSchedules);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy danh sách Job Schedule cho User ID {UserId}", userId);
                return StatusCode(500, new { message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        /// <summary>
        /// Lấy chi tiết đầy đủ 1 Job Schedule (bao gồm TestSuite và TestCase)
        /// </summary>
        [HttpGet("detail/{jobScheduleId}")]
        [Authorize]
        public async Task<ActionResult<JobScheduleApiTestEntity>> GetJobScheduleDetail(int jobScheduleId)
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized(new { message = "Không xác định được người dùng." });

            try
            {
                var jobSchedule = await _service.GetJobScheduleDetailAsync(jobScheduleId, userId.Value);

                if (jobSchedule == null)
                    return NotFound(new { message = "Không tìm thấy Job Schedule hoặc bạn không có quyền truy cập." });

                return Ok(jobSchedule);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy chi tiết Job Schedule ID {JobScheduleId} cho User ID {UserId}",
                    jobScheduleId, userId);
                return StatusCode(500, new { message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        /// <summary>
        /// Bật/Tắt trạng thái Job Schedule
        /// </summary>
        [HttpPatch("toggle-status/{jobScheduleId}")]
        [Authorize]
        public async Task<ActionResult> ToggleJobScheduleStatus(int jobScheduleId)
        {
            var userId = GetUserId();
            if (userId == null)
                return Unauthorized(new { message = "Không xác định được người dùng." });

            try
            {
                var result = await _service.ToggleJobScheduleStatusAsync(jobScheduleId, userId.Value);

                if (result == null)
                    return NotFound(new { message = "Không tìm thấy Job Schedule hoặc bạn không có quyền truy cập." });

                return Ok(new
                {
                    message = result.IsActive ? "Job đã được kích hoạt" : "Job đã bị tắt",
                    isActive = result.IsActive,
                    jobSchedule = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi toggle status Job Schedule ID {JobScheduleId} cho User ID {UserId}",
                    jobScheduleId, userId);
                return StatusCode(500, new { message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }
    }
}
