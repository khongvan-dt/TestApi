using AutoApiTester.App.Services;
using AutoApiTester.Models.DTOs;
using AutoApiTester.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AutoApiTester.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);

        if (!result.Success)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(result.Message));
        }

        // Lấy thông tin user
        var user = await _authService.GetUserByUsernameOrEmailAsync(dto.UsernameOrEmail);

        if (user == null)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("User not found"));
        }

        // Trả về cả token và user info
        var loginResponse = new
        {
            token = result.Data!.Token,
            expiresIn = result.Data.ExpiresIn,
            user = new
            {
                id = user.Id,
                username = user.Username,
                email = user.Email,
                fullName = user.FullName,
                isActive = user.IsActive,
                createdAt = user.CreatedAt
            }
        };

        return Ok(ApiResponse<object>.SuccessResponse(loginResponse, result.Message));
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto dto)
    {
        var result = await _authService.RegisterAsync(dto);

        if (!result.Success)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(result.Message));
        }

        return Ok(ApiResponse<object>.SuccessResponse(null, result.Message));
    }

    //  Thêm endpoint GetProfile
    [Authorize]
    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
        {
            return Unauthorized(ApiResponse<object>.ErrorResponse("Unauthorized"));
        }

        var userId = int.Parse(userIdClaim);
        var user = await _authService.GetUserByIdAsync(userId);

        if (user == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("User not found"));
        }

        return Ok(ApiResponse<object>.SuccessResponse(user));
    }
}