using FinanceManager.API.DTOs;
using FinanceManager.API.Hubs;
using FinanceManager.API.Services.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace FinanceManager.API.Services;

public class NotificationService : INotificationService
{
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IHubContext<NotificationHub> hubContext,
        ILogger<NotificationService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task SendNotificationToUserAsync(int userId, NotificationMessageDto notification)
    {
        try
        {
            var groupName = $"user_{userId}";
            await _hubContext.Clients.Group(groupName).SendAsync("ReceiveNotification", notification);
            await _hubContext.Clients.User(userId.ToString()).SendAsync("ReceiveNotification", notification);
            _logger.LogInformation("Sent notification '{Title}' to user {UserId}", notification.Title, userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send SignalR notification to user {UserId}", userId);
        }
    }

    public async Task BroadcastNotificationAsync(NotificationMessageDto notification)
    {
        try
        {
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", notification);
            _logger.LogInformation("Broadcasted notification '{Title}'", notification.Title);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to broadcast SignalR notification");
        }
    }

    public async Task SendTransactionAlertAsync(int userId, string title, string message, object? data = null)
    {
        var notification = new NotificationMessageDto
        {
            Title = title,
            Message = message,
            Type = "success",
            Data = data
        };

        await SendNotificationToUserAsync(userId, notification);
    }

    public async Task SendBudgetAlertAsync(int userId, string categoryName, decimal spent, decimal budgetLimit, decimal percentage)
    {
        var isExceeded = percentage >= 100;
        var notification = new NotificationMessageDto
        {
            Title = isExceeded ? "🚨 Budget Exceeded!" : "⚠️ Budget Warning",
            Message = isExceeded
                ? $"You have exceeded your monthly budget for '{categoryName}' (Spent: ₹{spent:N2} / Limit: ₹{budgetLimit:N2} - {percentage:F0}%)."
                : $"You have used {percentage:F0}% of your monthly budget for '{categoryName}' (Spent: ₹{spent:N2} / Limit: ₹{budgetLimit:N2}).",
            Type = isExceeded ? "danger" : "warning",
            Data = new
            {
                categoryName,
                spent,
                budgetLimit,
                percentage
            }
        };

        await SendNotificationToUserAsync(userId, notification);
    }

    public async Task SendBalanceUpdateAsync(int userId, int accountId, decimal newBalance)
    {
        try
        {
            var groupName = $"user_{userId}";
            await _hubContext.Clients.Group(groupName).SendAsync("BalanceUpdated", new
            {
                accountId,
                newBalance,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send BalanceUpdated signal to user {UserId}", userId);
        }
    }
}
