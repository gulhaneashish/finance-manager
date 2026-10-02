using FinanceManager.API.DTOs;

namespace FinanceManager.API.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync(
        int userId,
        string? period = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int? year = null,
        int? month = null);

    Task<List<CategorySpendingDto>> GetCategorySpendingAsync(
        int userId,
        string? period = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int? year = null,
        int? month = null);

    Task<List<MonthlyCashFlowDto>> GetMonthlyCashFlowAsync(
        int userId,
        int year);

    Task<LoanDebtSummaryDto> GetLoanDebtSummaryAsync(
        int userId);

    Task<List<AccountSummaryDto>> GetAccountSummaryAsync(
        int userId);

    Task<SavingsInvestmentSummaryDto> GetSavingsInvestmentSummaryAsync(
        int userId,
        string? period = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int? year = null,
        int? month = null);
}
