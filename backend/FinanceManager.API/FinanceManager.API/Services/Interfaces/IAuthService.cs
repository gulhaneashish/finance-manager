using FinanceManager.API.DTOs;

namespace FinanceManager.API.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginDto dto);
    Task<LoginResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto dto);
    Task<bool> RevokeTokenAsync(int userId);
    Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto dto);
}
