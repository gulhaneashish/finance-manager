using FinanceManager.API.DTOs;
using FinanceManager.API.Models;

namespace FinanceManager.API.Services.Interfaces;

public interface IUserService
{
    Task<List<User>> GetUsersAsync();
    Task<UserResponseDto> CreateUserAsync(UserCreateDto dto);
    Task<User?> GetUserByIdAsync(int userId);
    Task<UserProfileDto?> GetProfileAsync(int userId);
    Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateProfileDto dto);
}
