using FinanceManager.API.Data;
using FinanceManager.API.DTOs;
using FinanceManager.API.Models;
using FinanceManager.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.API.Services;

public class AdminService : IAdminService
{
    private readonly FinanceDbContext _context;
    private readonly IAuditLogService _auditLogService;

    public AdminService(
        FinanceDbContext context,
        IAuditLogService auditLogService)
    {
        _context = context;
        _auditLogService = auditLogService;
    }

    // ==========================================
    // 1. MANAGE USERS (Strict Privacy: No Financial/Transaction Data)
    // ==========================================
    public async Task<IEnumerable<AdminUserDto>> GetAllUsersAsync()
    {
        return await _context.Users
            .AsNoTracking()
            .OrderBy(u => u.Id)
            .Select(u => new AdminUserDto
            {
                Id = u.Id,
                Name = u.Name,
                Email = u.Email,
                Role = string.IsNullOrWhiteSpace(u.Role) ? "User" : u.Role,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<bool> UpdateUserStatusAsync(int targetUserId, bool isActive, int adminUserId, string adminEmail)
    {
        var user = await _context.Users.FindAsync(targetUserId);
        if (user == null)
        {
            return false;
        }

        if (targetUserId == adminUserId && !isActive)
        {
            throw new InvalidOperationException("You cannot deactivate your own admin account.");
        }

        user.IsActive = isActive;
        if (!isActive)
        {
            // Invalidate refresh tokens immediately upon deactivation
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
        }

        await _context.SaveChangesAsync();

        var statusText = isActive ? "ACTIVATED" : "DEACTIVATED";
        await _auditLogService.LogAsync(
            adminUserId,
            adminEmail,
            "USER_STATUS_CHANGE",
            $"User '{user.Email}' (ID: {user.Id}) was {statusText} by admin.");

        return true;
    }

    public async Task<bool> UpdateUserRoleAsync(int targetUserId, string role, int adminUserId, string adminEmail)
    {
        var user = await _context.Users.FindAsync(targetUserId);
        if (user == null)
        {
            return false;
        }

        var normalizedRole = role.Equals("Admin", StringComparison.OrdinalIgnoreCase) ? "Admin" : "User";

        if (targetUserId == adminUserId && normalizedRole != "Admin")
        {
            throw new InvalidOperationException("You cannot remove the Admin role from your own account.");
        }

        user.Role = normalizedRole;
        await _context.SaveChangesAsync();

        await _auditLogService.LogAsync(
            adminUserId,
            adminEmail,
            "USER_ROLE_CHANGE",
            $"User '{user.Email}' (ID: {user.Id}) role set to '{normalizedRole}' by admin.");

        return true;
    }

    // ==========================================
    // 2. MANAGE EXPENSE CATEGORIES
    // ==========================================
    public async Task<IEnumerable<AdminCategoryDto>> GetAllCategoriesAsync()
    {
        return await _context.Categories
            .AsNoTracking()
            .Include(c => c.User)
            .OrderBy(c => c.Type)
            .ThenBy(c => c.Name)
            .Select(c => new AdminCategoryDto
            {
                Id = c.Id,
                Name = c.Name,
                Type = c.Type,
                UserId = c.UserId,
                UserName = c.User != null ? c.User.Name : "System",
                UserEmail = c.User != null ? c.User.Email : "system@local",
                TransactionCount = 0
            })
            .ToListAsync();
    }

    public async Task<AdminCategoryDto> CreateCategoryAsync(AdminCreateCategoryDto dto, int adminUserId, string adminEmail)
    {
        var targetUserId = dto.TargetUserId.HasValue && dto.TargetUserId.Value > 0
            ? dto.TargetUserId.Value
            : adminUserId;

        var type = string.Equals(dto.Type, "INCOME", StringComparison.OrdinalIgnoreCase) ? "INCOME" : "EXPENSE";

        var category = new Category
        {
            Name = dto.Name.Trim(),
            Type = type,
            UserId = targetUserId
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync();

        var owner = await _context.Users.FindAsync(targetUserId);

        await _auditLogService.LogAsync(
            adminUserId,
            adminEmail,
            "CATEGORY_CREATED",
            $"Created {type} category '{category.Name}' (ID: {category.Id}).");

        return new AdminCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Type = category.Type,
            UserId = category.UserId,
            UserName = owner?.Name ?? "Admin",
            UserEmail = owner?.Email ?? adminEmail,
            TransactionCount = 0
        };
    }

    public async Task<AdminCategoryDto?> UpdateCategoryAsync(int categoryId, AdminUpdateCategoryDto dto, int adminUserId, string adminEmail)
    {
        var category = await _context.Categories
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == categoryId);

        if (category == null)
        {
            return null;
        }

        var oldName = category.Name;
        var oldType = category.Type;

        category.Name = dto.Name.Trim();
        category.Type = string.Equals(dto.Type, "INCOME", StringComparison.OrdinalIgnoreCase) ? "INCOME" : "EXPENSE";

        await _context.SaveChangesAsync();

        await _auditLogService.LogAsync(
            adminUserId,
            adminEmail,
            "CATEGORY_UPDATED",
            $"Updated category ID: {category.Id} from '{oldName}' ({oldType}) to '{category.Name}' ({category.Type}).");

        return new AdminCategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Type = category.Type,
            UserId = category.UserId,
            UserName = category.User?.Name ?? "System",
            UserEmail = category.User?.Email ?? "system@local",
            TransactionCount = 0
        };
    }

    public async Task<bool> DeleteCategoryAsync(int categoryId, int adminUserId, string adminEmail)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == categoryId);

        if (category == null)
        {
            return false;
        }

        var linkedBudgets = await _context.CategoryBudgets.Where(cb => cb.CategoryId == categoryId).ToListAsync();
        if (linkedBudgets.Any())
        {
            _context.CategoryBudgets.RemoveRange(linkedBudgets);
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();

        await _auditLogService.LogAsync(
            adminUserId,
            adminEmail,
            "CATEGORY_DELETED",
            $"Deleted category '{category.Name}' (ID: {categoryId}, Type: {category.Type}).");

        return true;
    }

    // ==========================================
    // 3. SYSTEM-WIDE ADMINISTRATIVE STATISTICS (Operational Only)
    // ==========================================
    public async Task<AdminSystemStatsDto> GetSystemStatsAsync()
    {
        var totalUsers = await _context.Users.CountAsync();
        var activeUsers = await _context.Users.CountAsync(u => u.IsActive);
        var deactivatedUsers = totalUsers - activeUsers;

        var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var newUsersThisMonth = await _context.Users.CountAsync(u => u.CreatedAt >= startOfMonth);

        var totalCategories = await _context.Categories.CountAsync();
        var totalExpenseCategories = await _context.Categories.CountAsync(c => c.Type == "EXPENSE");
        var totalIncomeCategories = await _context.Categories.CountAsync(c => c.Type == "INCOME");
        var totalAuditLogs = await _context.AuditLogs.CountAsync();

        // Operational administrative activity stream (strictly security & audit logs, NO user transactions)
        var recentActivities = await _context.AuditLogs
            .AsNoTracking()
            .OrderByDescending(a => a.Timestamp)
            .Take(8)
            .Select(a => new AdminRecentActivityDto
            {
                Title = a.Action,
                Details = $"{a.UserEmail}: {a.Details}",
                Type = "AUDIT",
                Timestamp = a.Timestamp
            })
            .ToListAsync();

        return new AdminSystemStatsDto
        {
            TotalUsers = totalUsers,
            ActiveUsers = activeUsers,
            DeactivatedUsers = deactivatedUsers,
            NewUsersThisMonth = newUsersThisMonth,
            TotalCategories = totalCategories,
            TotalExpenseCategories = totalExpenseCategories,
            TotalIncomeCategories = totalIncomeCategories,
            TotalAuditLogs = totalAuditLogs,
            RecentActivities = recentActivities
        };
    }

    // ==========================================
    // 4. AUDIT LOGS
    // ==========================================
    public async Task<IEnumerable<AuditLogDto>> GetAuditLogsAsync(int limit = 200)
    {
        return await _auditLogService.GetLogsAsync(limit);
    }
}
