using FinanceManager.API.DTOs;

namespace FinanceManager.API.Services.Interfaces;

public interface IReportService
{
    Task<ReportSummaryDto> GetSummaryAsync(int userId, DateTime? fromDate, DateTime? toDate);
}
