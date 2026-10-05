using FinanceManager.API.DTOs;
using FinanceManager.API.Models;
using FinanceManager.API.Repositories.Interfaces;
using FinanceManager.API.Services.Interfaces;

namespace FinanceManager.API.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IAuditLogService _auditLogService;

    public UserService(
        IUserRepository userRepository,
        ICategoryRepository categoryRepository,
        IAuditLogService auditLogService)
    {
        _userRepository = userRepository;
        _categoryRepository = categoryRepository;
        _auditLogService = auditLogService;
    }

    public async Task<List<User>> GetUsersAsync()
    {
        return await _userRepository.GetAllAsync();
    }

    public async Task<UserResponseDto> CreateUserAsync(UserCreateDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new InvalidOperationException("Full Name is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.Email))
        {
            throw new InvalidOperationException("Email address is required.");
        }

        if (string.IsNullOrWhiteSpace(dto.Password) || dto.Password.Length < 6)
        {
            throw new InvalidOperationException("Password must be at least 6 characters long.");
        }

        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
        var existingUser = await _userRepository.GetByEmailAsync(normalizedEmail);

        if (existingUser != null)
        {
            throw new InvalidOperationException("An account with this email address already exists.");
        }

        var user = new User
        {
            Name = dto.Name.Trim(),
            Email = normalizedEmail,
            Username = normalizedEmail.Split('@')[0],
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            Role = "User",
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        // Seed default starter categories for instant usability
        var defaultCategories = new List<Category>
        {
            new() { UserId = user.Id, Name = "Salary", Type = "INCOME" },
            new() { UserId = user.Id, Name = "Freelance / Side Gig", Type = "INCOME" },
            new() { UserId = user.Id, Name = "Investments & Dividends", Type = "INCOME" },
            new() { UserId = user.Id, Name = "Other Income", Type = "INCOME" },
            new() { UserId = user.Id, Name = "Food & Dining", Type = "EXPENSE" },
            new() { UserId = user.Id, Name = "Groceries", Type = "EXPENSE" },
            new() { UserId = user.Id, Name = "Housing & Rent", Type = "EXPENSE" },
            new() { UserId = user.Id, Name = "Utilities & Bills", Type = "EXPENSE" },
            new() { UserId = user.Id, Name = "Transportation & Fuel", Type = "EXPENSE" },
            new() { UserId = user.Id, Name = "Shopping", Type = "EXPENSE" },
            new() { UserId = user.Id, Name = "Healthcare & Medical", Type = "EXPENSE" },
            new() { UserId = user.Id, Name = "Entertainment & Leisure", Type = "EXPENSE" }
        };

        foreach (var category in defaultCategories)
        {
            await _categoryRepository.AddAsync(category);
        }
        await _categoryRepository.SaveChangesAsync();

        // Record security audit log
        await _auditLogService.LogAsync(
            user.Id,
            user.Email,
            "USER_REGISTERED",
            $"New user registration completed successfully for '{user.Name}' ({user.Email}). Starter categories provisioned.");

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