namespace FinanceManager.API.DTOs;

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;

    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiresAt { get; set; }

    public int UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Role { get; set; } = "User";
}