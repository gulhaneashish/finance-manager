using FinanceManager.API.DTOs;

namespace FinanceManager.API.Services.Interfaces;

public interface IAdminService
{
    // Users
    Task<IEnumerable<AdminUserDto>> GetAllUsersAsync();
    Task<bool> UpdateUserStatusAsync(int targetUserId, bool isActive, int adminUserId, string adminEmail);
    Task<bool> UpdateUserRoleAsync(int targetUserId, string role, int adminUserId, string adminEmail);

    // Categories
    Task<IEnumerable<AdminCategoryDto>> GetAllCategoriesAsync();
    Task<AdminCategoryDto> CreateCategoryAsync(AdminCreateCategoryDto dto, int adminUserId, string adminEmail);
    Task<AdminCategoryDto?> UpdateCategoryAsync(int categoryId, AdminUpdateCategoryDto dto, int adminUserId, string adminEmail);
    Task<bool> DeleteCategoryAsync(int categoryId, int adminUserId, string adminEmail);

    // Statistics
    Task<AdminSystemStatsDto> GetSystemStatsAsync();

    // Audit Logs
    Task<IEnumerable<AuditLogDto>> GetAuditLogsAsync(int limit = 200);
}
