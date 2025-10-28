using AutoApiTester.Models;

namespace AutoApiTester.App.Services
{
    public interface IJwtService
    {
        string GenerateToken(User user, int expiresMinutes = 1440);
        int? GetUserIdFromToken(string token);
    }
}
