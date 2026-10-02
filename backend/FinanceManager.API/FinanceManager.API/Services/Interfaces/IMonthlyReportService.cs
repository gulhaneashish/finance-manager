using FinanceManager.API.DTOs;

namespace FinanceManager.API.Services.Interfaces;

public interface IMonthlyReportService
{
    Task<MonthlyReportDto> GetReportAsync(int userId, int year, int month);
}
