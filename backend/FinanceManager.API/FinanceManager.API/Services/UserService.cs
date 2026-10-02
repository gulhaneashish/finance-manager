using FinanceManager.API.DTOs;
using FinanceManager.API.Models;
using FinanceManager.API.Repositories.Interfaces;
using FinanceManager.API.Services.Interfaces;

namespace FinanceManager.API.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<List<User>> GetUsersAsync()
    {
        return await _userRepository.GetAllAsync();
    }

    public async Task<UserResponseDto> CreateUserAsync(UserCreateDto dto)
    {
        var existingUser = await _userRepository.GetByEmailAsync(dto.Email);

        if (existingUser != null)
        {
            throw new Exception("Email already registered.");
        }

        var user = new User
        {
            Name = dto.Name,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        return new UserResponseDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        return await _userRepository.GetByIdAsync(userId);
    }

    public async Task<UserProfileDto?> GetProfileAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            return null;
        }

        var username = string.IsNullOrWhiteSpace(user.Username)
            ? user.Email.Split('@')[0]
            : user.Username;

        return new UserProfileDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Username = username,
            PhoneNumber = user.PhoneNumber,
            ProfilePictureUrl = user.ProfilePictureUrl,
            Role = string.IsNullOrWhiteSpace(user.Role) ? "User" : user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }

    public async Task<UserProfileDto> UpdateProfileAsync(int userId, UpdateProfileDto dto)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            throw new KeyNotFoundException("User not found.");
        }

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new InvalidOperationException("Full Name cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            throw new InvalidOperationException("Email cannot be empty.");
        }

        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        if (!string.Equals(user.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase))
        {
            var existingWithEmail = await _userRepository.GetByEmailAsync(normalizedEmail);
            if (existingWithEmail != null && existingWithEmail.Id != user.Id)
            {
                throw new InvalidOperationException("This email address is already in use by another account.");
            }
            user.Email = normalizedEmail;
        }

        user.Name = dto.Name.Trim();
        user.PhoneNumber = string.IsNullOrWhiteSpace(dto.PhoneNumber) ? null : dto.PhoneNumber.Trim();
        user.ProfilePictureUrl = string.IsNullOrWhiteSpace(dto.ProfilePictureUrl) ? null : dto.ProfilePictureUrl.Trim();

        // Note: Username, Role, IsActive, and LastLoginAt are strictly read-only and preserved
        if (string.IsNullOrWhiteSpace(user.Username))
        {
            user.Username = user.Email.Split('@')[0];
        }

        await _userRepository.SaveChangesAsync();

        return new UserProfileDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Username = user.Username,
            PhoneNumber = user.PhoneNumber,
            ProfilePictureUrl = user.ProfilePictureUrl,
            Role = string.IsNullOrWhiteSpace(user.Role) ? "User" : user.Role,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt
        };
    }
}