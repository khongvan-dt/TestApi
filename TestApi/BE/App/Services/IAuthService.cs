using AutoApiTester.DTOs.Auth;
using AutoApiTester.Models.DTOs;

namespace AutoApiTester.App.Services;

public interface IAuthService  
{
    Task<(bool Success, string Message, LoginResultDto? Data)> LoginAsync(LoginDto dto);
    Task<(bool Success, string Message)> RegisterAsync(RegisterDto dto);
    Task<UserInfoDto?> GetUserByIdAsync(int userId);
    Task<UserInfoDto?> GetUserByUsernameOrEmailAsync(string usernameOrEmail); 
}