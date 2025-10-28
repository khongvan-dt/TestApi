using AutoApiTester.App.Repositories;
using AutoApiTester.App.Services;
using AutoApiTester.Data;
using AutoApiTester.Models;
using AutoApiTester.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace AutoApiTester.Repositories
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IJwtService _jwtService;
        private readonly IConfiguration _config;

        public UserRepository(ApplicationDbContext context, IJwtService jwtService, IConfiguration config)
            : base(context)
        {
            _context = context;
            _jwtService = jwtService;
            _config = config;
        }

        public async Task<(bool Success, string Message, LoginResultDto? Data)> LoginAsync(LoginDto dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == dto.UsernameOrEmail || u.Email == dto.UsernameOrEmail);

            if (user == null)
                return (false, "Invalid username/email or password", null);

            var hashedPassword = HashPassword(dto.Password);
            if (user.PasswordHash != hashedPassword)
                return (false, "Invalid username/email or password", null);

            if (!user.IsActive)
                return (false, "Account is disabled", null);

            var expiresMinutes = int.Parse(_config["Jwt:ExpiresMinutes"] ?? "1440");
            var token = _jwtService.GenerateToken(user, expiresMinutes);

            var result = new LoginResultDto(token, expiresMinutes * 60);

            return (true, "Login successful", result);
        }

        public async Task<(bool Success, string Message)> RegisterAsync(RegisterDto dto)
        {
            if (await _context.Users.AnyAsync(u => u.Username == dto.Username))
                return (false, "Username already exists");

            if (await _context.Users.AnyAsync(u => u.Email == dto.Email))
                return (false, "Email already exists");

            var user = new User
            {
                 Username = dto.Username,
                Email = dto.Email,
                PasswordHash = HashPassword(dto.Password),
                FullName = dto.FullName,
                IsActive = true
            };

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            return (true, "Registration successful");
        }

        public async Task<User?> GetUserByIdAsync(int userId)
        {
            return await _context.Users.FindAsync(userId);
        }

        // ✅ Thêm method này
        public async Task<User?> GetUserByUsernameOrEmailAsync(string usernameOrEmail)
        {
            return await _context.Users
                .FirstOrDefaultAsync(u => u.Username == usernameOrEmail || u.Email == usernameOrEmail);
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(bytes);
        }
    }
}