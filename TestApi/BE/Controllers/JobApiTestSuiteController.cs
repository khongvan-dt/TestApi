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

        [HttpPost("upsert")]
        [Authorize]
        public async Task<ActionResult<List<JobApiTestSuiteEntity>>> UpsertAsync([FromBody] List<JobApiTestSuiteDto> dto)
        {
            if (dto == null)
                return BadRequest(new { message = "Dữ liệu không hợp lệ" });

            var userName = User?.Identity?.Name ?? "system";

            try
            {
                var result = await _service.UpsertAsync(dto, userName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error when upserting JobApiTestSuite");
                return StatusCode(500, new { message = "Có lỗi xảy ra khi lưu JobApiTestSuite" });
            }
        }

        ///// <summary>
        ///// Lấy danh sách tất cả JobApiTestSuite
        ///// </summary>
        //[HttpGet]
        //public async Task<ActionResult<IEnumerable<JobApiTestSuiteEntity>>> GetAll()
        //{
        //    var result = await _service.GetAllAsync();
        //    return Ok(result);
        //}

        ///// <summary>
        ///// Lấy chi tiết theo ID
        ///// </summary>
        //[HttpGet("{id}")]
        //public async Task<ActionResult<JobApiTestSuiteEntity>> GetById(int id)
        //{
        //    var item = await _service.GetByIdAsync(id);
        //    if (item == null)
        //        return NotFound(new { message = "Không tìm thấy JobApiTestSuite" });

        //    return Ok(item);
        //}

        ///// <summary>
        ///// Xóa 1 JobApiTestSuite
        ///// </summary>
        //[HttpDelete("{id}")]
        //public async Task<IActionResult> Delete(int id)
        //{
        //    var deleted = await _service.DeleteAsync(id);
        //    if (!deleted)
        //        return NotFound(new { message = "Không tìm thấy để xóa" });

        //    return Ok(new { message = "Đã xóa thành công" });
        //}
    }
}
