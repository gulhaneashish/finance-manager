using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using FinanceManager.API.DTOs;
using FinanceManager.API.Repositories.Interfaces;
using FinanceManager.API.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace FinanceManager.API.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IConfiguration _configuration;
    private readonly IAuditLogService _auditLogService;

    public AuthService(
        IUserRepository userRepository,
        IConfiguration configuration,
        IAuditLogService auditLogService)
    {
        _userRepository = userRepository;
        _configuration = configuration;
        _auditLogService = auditLogService;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginDto dto)
    {
        var user = await _userRepository.GetByEmailAsync(dto.Email);

        if (user == null)
        {
            return null;
        }

        if (!user.IsActive)
        {
            throw new InvalidOperationException("Your account has been deactivated. Please contact an administrator.");
        }

        var passwordValid = BCrypt.Net.BCrypt.Verify(
            dto.Password,
            user.PasswordHash);

        if (!passwordValid)
        {
            return null;
        }

        var token = GenerateToken(user);
        var refreshToken = GenerateRefreshToken();
        var expiryDays = GetRefreshTokenExpirationDays();
        var refreshTokenExpiry = DateTime.UtcNow.AddDays(expiryDays);

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = refreshTokenExpiry;
        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.SaveChangesAsync();

        return new LoginResponseDto
        {
            Token = token,
            RefreshToken = refreshToken,
            RefreshTokenExpiresAt = refreshTokenExpiry,
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = string.IsNullOrWhiteSpace(user.Role) ? "User" : user.Role
        };
    }

    public async Task<LoginResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Token) || string.IsNullOrWhiteSpace(dto.RefreshToken))
        {
            return null;
        }

        var principal = GetPrincipalFromExpiredToken(dto.Token);
        if (principal == null)
        {
            return null;
        }

        var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return null;
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
{
    Console.WriteLine($"Refresh failed: User {userId} not found.");
    return null;
}

if (!user.IsActive)
{
    Console.WriteLine($"Refresh failed: User {userId} is inactive.");
    return null;
}

if (user.RefreshToken != dto.RefreshToken)
{
    Console.WriteLine("Refresh failed: Refresh token does not match.");
    return null;
}

if (user.RefreshTokenExpiryTime == null)
{
    Console.WriteLine("Refresh failed: Refresh token expiry is null.");
    return null;
}

if (user.RefreshTokenExpiryTime <= DateTime.UtcNow)
{
    Console.WriteLine($"Refresh failed: Refresh token expired at {user.RefreshTokenExpiryTime}.");
    return null;
}

        // Refresh token rotation: issue a new access token and a new refresh token
        var newAccessToken = GenerateToken(user);
        var newRefreshToken = GenerateRefreshToken();
        var expiryDays = GetRefreshTokenExpirationDays();
        var newRefreshTokenExpiry = DateTime.UtcNow.AddDays(expiryDays);

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = newRefreshTokenExpiry;
        await _userRepository.SaveChangesAsync();

        return new LoginResponseDto
        {
            Token = newAccessToken,
            RefreshToken = newRefreshToken,
            RefreshTokenExpiresAt = newRefreshTokenExpiry,
            UserId = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = string.IsNullOrWhiteSpace(user.Role) ? "User" : user.Role
        };
    }

    public async Task<bool> RevokeTokenAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return false;
        }

        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        await _userRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.CurrentPassword) ||
            string.IsNullOrWhiteSpace(dto.NewPassword) ||
            string.IsNullOrWhiteSpace(dto.ConfirmNewPassword))
        {
            throw new InvalidOperationException("Current password, new password, and confirmation are required.");
        }

        if (dto.NewPassword != dto.ConfirmNewPassword)
        {
            throw new InvalidOperationException("New password and confirmation do not match.");
        }

        if (dto.NewPassword.Length < 8)
        {
            throw new InvalidOperationException("New password must be at least 8 characters long.");
        }

        if (!dto.NewPassword.Any(char.IsUpper) || !dto.NewPassword.Any(char.IsLower) || !dto.NewPassword.Any(char.IsDigit))
        {
            throw new InvalidOperationException("New password must contain at least one uppercase letter, one lowercase letter, and one number.");
        }

        if (dto.CurrentPassword == dto.NewPassword)
        {
            throw new InvalidOperationException("New password cannot be the same as your current password.");
        }

        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        if (!user.IsActive)
        {
            throw new InvalidOperationException("Account is inactive.");
        }

        var isCurrentValid = BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash);
        if (!isCurrentValid)
        {
            throw new InvalidOperationException("Current password is incorrect.");
        }

        // Hash new password using BCrypt
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

        // Invalidate active refresh tokens to force re-authentication on other devices/sessions
        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;

        await _userRepository.SaveChangesAsync();

        // Security audit logging
        await _auditLogService.LogAsync(
            user.Id,
            user.Email,
            "PASSWORD_CHANGED",
            "User successfully changed their password. Active sessions invalidated.");

        return true;
    }

    private string GetJwtKey()
    {
        var key = Environment.GetEnvironmentVariable("JWT_KEY")
            ?? Environment.GetEnvironmentVariable("JWT__KEY")
            ?? _configuration["Jwt:Key"];

        if (string.IsNullOrWhiteSpace(key))
        {
            throw new InvalidOperationException("CRITICAL: JWT Secret Key is not configured in the application environment.");
        }
        return key;
    }

    private string GetJwtIssuer()
    {
        return Environment.GetEnvironmentVariable("JWT_ISSUER")
            ?? _configuration["Jwt:Issuer"]
            ?? "FinanceManager.API";
    }

    private string GetJwtAudience()
    {
        return Environment.GetEnvironmentVariable("JWT_AUDIENCE")
            ?? _configuration["Jwt:Audience"]
            ?? "FinanceManager.Client";
    }

    private string GenerateToken(Models.User user)
    {
        var key = GetJwtKey();
        var issuer = GetJwtIssuer();
        var audience = GetJwtAudience();

        var expiryConfig = Environment.GetEnvironmentVariable("JWT_EXPIRATION_MINUTES")
            ?? _configuration["Jwt:ExpirationMinutes"];
        var expiryMinutes = double.TryParse(expiryConfig, out var parsed) ? parsed : 60.0;

        var role = string.IsNullOrWhiteSpace(user.Role) ? "User" : user.Role;
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, role)
        };

        var securityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(key));

        var credentials = new SigningCredentials(
            securityKey,
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiryMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    private int GetRefreshTokenExpirationDays()
    {
        var daysConfig = Environment.GetEnvironmentVariable("JWT_REFRESH_EXPIRATION_DAYS")
            ?? _configuration["Jwt:RefreshTokenExpirationDays"];
        return int.TryParse(daysConfig, out var days) ? days : 7;
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = true,
            ValidAudience = GetJwtAudience(),
            ValidateIssuer = true,
            ValidIssuer = GetJwtIssuer(),
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(GetJwtKey())),
            ValidateLifetime = false // Keep false so we can extract claims from expired JWT
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch
        {
            return null;
        }
    }
}