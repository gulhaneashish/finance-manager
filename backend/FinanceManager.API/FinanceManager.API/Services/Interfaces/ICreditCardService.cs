using FinanceManager.API.DTOs;
using FinanceManager.API.Models;

namespace FinanceManager.API.Services.Interfaces;

public interface ICreditCardService
{
    Task<Transaction> MakePurchaseAsync(CreditCardPurchaseDto dto, int userId);
    Task<decimal> GetOutstandingAsync(int cardId, int userId);
    Task<Transaction> MakePaymentAsync(CreditCardPaymentDto dto, int userId);
}
