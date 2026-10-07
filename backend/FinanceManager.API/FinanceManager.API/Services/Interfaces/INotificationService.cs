using FinanceManager.API.DTOs;

namespace FinanceManager.API.Services.Interfaces;

public interface INotificationService
{
    Task SendNotificationToUserAsync(int userId, NotificationMessageDto notification);
    Task BroadcastNotificationAsync(NotificationMessageDto notification);
    Task SendTransactionAlertAsync(int userId, string title, string message, object? data = null);
    Task SendBudgetAlertAsync(int userId, string categoryName, decimal spent, decimal budgetLimit, decimal percentage);
    Task SendBalanceUpdateAsync(int userId, int accountId, decimal newBalance);
}
