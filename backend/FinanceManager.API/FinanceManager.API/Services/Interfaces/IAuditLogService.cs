using FinanceManager.API.DTOs;
using FinanceManager.API.Models;

namespace FinanceManager.API.Services.Interfaces;

public interface IAuditLogService
{
    Task LogAsync(int? userId, string userEmail, string action, string details);
    Task<IEnumerable<AuditLogDto>> GetLogsAsync(int limit = 200);
}
