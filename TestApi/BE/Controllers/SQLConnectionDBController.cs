using AutoApiTester.App.Services;
using AutoApiTester.Models;
using AutoApiTester.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace AutoApiTester.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SQLConnectionDBController : ControllerBase
    {
        private readonly ISQLConnectionDBService _service;

        public SQLConnectionDBController(ISQLConnectionDBService service)
        {
            _service = service;
        }

        // ✅ POST: api/SQLConnectionDB/save
        [HttpPost("save")]
        public async Task<IActionResult> Save([FromBody] SQLConnectionDBEntity dto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                if (userId == 0)
                    return Unauthorized(new { success = false, message = "User not authenticated" });

                var result = await _service.SaveAsync(dto, userId);

                return Ok(new
                {
                    success = true,
                    data = result,
                    message =  dto.Id > 0
                        ? "Connection updated successfully"
                        : "Connection created successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ✅ GET: api/SQLConnectionDB
        [HttpGet]
        public async Task<IActionResult> GetByUserId()
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                if (userId == 0)
                    return Unauthorized(new { success = false, message = "User not authenticated" });

                var connections = await _service.GetByUserIdAsync(userId);

                return Ok(new
                {
                    success = true,
                    data = connections,
                    message = "Connections retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ✅ GET: api/SQLConnectionDB/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                if (userId == 0)
                    return Unauthorized(new { success = false, message = "User not authenticated" });

                var connection = await _service.GetByIdAsync(id, userId);

                return Ok(new
                {
                    success = true,
                    data = connection,
                    message = "Connection retrieved successfully"
                });
            }
            catch (Exception ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
        }

        // ✅ DELETE: api/SQLConnectionDB/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

                if (userId == 0)
                    return Unauthorized(new { success = false, message = "User not authenticated" });

                var result = await _service.DeleteAsync(id, userId);

                if (!result)
                    return NotFound(new { success = false, message = "Connection not found" });

                return Ok(new
                {
                    success = true,
                    message = "Connection deleted successfully"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }

        // ✅ POST: api/SQLConnectionDB/test
        [HttpPost("test")]
        public async Task<IActionResult> TestConnection([FromBody] TestConnectionDto dto)
        {
            try
            {
                var result = await _service.TestConnectionAsync(dto.ConnectionString);

                return Ok(new
                {
                    success = result,
                    message = result
                        ? "Connection successful"
                        : "Connection failed"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }

    public class TestConnectionDto
    {
        public string ConnectionString { get; set; }
    }
}