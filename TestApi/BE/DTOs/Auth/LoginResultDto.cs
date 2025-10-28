namespace AutoApiTester.Models.DTOs;

public class LoginResultDto
{
    public string Token { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }

    public LoginResultDto(string token, int expiresIn)
    {
        Token = token;
        ExpiresIn = expiresIn;
    }
}