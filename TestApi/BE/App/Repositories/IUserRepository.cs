using AutoApiTester.Models;
using AutoApiTester.Models.DTOs;

namespace AutoApiTester.App.Repositories;

public interface IUserRepository : IRepository<UserEntity>
{
    Task<(bool Success, string Message, LoginResultDto? Data)> LoginAsync(LoginDto dto);
    Task<(bool Success, string Message)> RegisterAsync(RegisterDto dto);
    Task<UserEntity?> GetUserByIdAsync(int userId);
    Task<UserEntity?> GetUserByUsernameOrEmailAsync(string usernameOrEmail); // ✅ Thêm dòng này
}