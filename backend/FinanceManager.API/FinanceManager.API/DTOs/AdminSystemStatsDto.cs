namespace FinanceManager.API.DTOs;

public class AdminSystemStatsDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int DeactivatedUsers { get; set; }
    public int NewUsersThisMonth { get; set; }
    public int TotalCategories { get; set; }
    public int TotalExpenseCategories { get; set; }
    public int TotalIncomeCategories { get; set; }
    public int TotalAuditLogs { get; set; }
    public List<AdminRecentActivityDto> RecentActivities { get; set; } = new();
}

public class AdminRecentActivityDto
{
    public string Title { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
