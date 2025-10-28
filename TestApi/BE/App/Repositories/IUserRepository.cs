using AutoApiTester.Models;
using AutoApiTester.Models.DTOs;

namespace AutoApiTester.App.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<(bool Success, string Message, LoginResultDto? Data)> LoginAsync(LoginDto dto);
    Task<(bool Success, string Message)> RegisterAsync(RegisterDto dto);
    Task<User?> GetUserByIdAsync(int userId);
    Task<User?> GetUserByUsernameOrEmailAsync(string usernameOrEmail); // ✅ Thêm dòng này
}