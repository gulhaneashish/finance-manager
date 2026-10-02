namespace FinanceManager.API.Services.Interfaces;

public interface IAccountBalanceService
{
    Task<decimal> GetAccountBalanceAsync(int accountId, int userId);
}
