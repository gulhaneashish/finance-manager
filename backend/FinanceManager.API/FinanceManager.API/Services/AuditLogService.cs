using FinanceManager.API.Data;
using FinanceManager.API.DTOs;
using FinanceManager.API.Models;
using FinanceManager.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.API.Services;

public class AuditLogService : IAuditLogService
{
    private readonly FinanceDbContext _context;

    public AuditLogService(FinanceDbContext context)
    {
        _context = context;
    }

    public async Task LogAsync(int? userId, string userEmail, string action, string details)
    {
        try
        {
            var log = new AuditLog
            {
                UserId = userId,
                UserEmail = userEmail ?? string.Empty,
                Action = action,
                Details = details,
                Timestamp = DateTime.UtcNow
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Logging failure should not break critical transactions, but log to console
            Console.WriteLine($"[AuditLogService Error] Failed to write audit log: {ex.Message}");
        }
    }

    public async Task<IEnumerable<AuditLogDto>> GetLogsAsync(int limit = 200)
    {
        return await _context.AuditLogs
            .AsNoTracking()
            .OrderByDescending(l => l.Timestamp)
            .Take(limit)
            .Select(l => new AuditLogDto
            {
                Id = l.Id,
                UserId = l.UserId,
                UserEmail = l.UserEmail,
                Action = l.Action,
                Details = l.Details,
                Timestamp = l.Timestamp
            })
            .ToListAsync();
    }
}
