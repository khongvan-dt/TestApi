using AutoApiTester.App.Repositories;
using AutoApiTester.App.Services;
using AutoApiTester.DTOs.Auth;
using AutoApiTester.Models;
using AutoApiTester.Models.DTOs;

namespace AutoApiTester.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepo;

        public AuthService(IUserRepository userRepo)
        {
            _userRepo = userRepo;
        }

        public async Task<(bool Success, string Message, LoginResultDto? Data)> LoginAsync(LoginDto dto)
        {
            return await _userRepo.LoginAsync(dto);
        }

        public async Task<(bool Success, string Message)> RegisterAsync(RegisterDto dto)
        {
            return await _userRepo.RegisterAsync(dto);
        }

        public async Task<UserInfoDto?> GetUserByIdAsync(int userId)
        {
            var user = await _userRepo.GetUserByIdAsync(userId);
            if (user == null) return null;

            return new UserInfoDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }

        // ✅ Thêm method này
        public async Task<UserInfoDto?> GetUserByUsernameOrEmailAsync(string usernameOrEmail)
        {
            var user = await _userRepo.GetUserByUsernameOrEmailAsync(usernameOrEmail);
            if (user == null) return null;

            return new UserInfoDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt
            };
        }
    }
}