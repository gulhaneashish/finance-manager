using FinanceManager.API.Data;
using FinanceManager.API.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.API.Services;

public class AccountBalanceService
{
    private readonly FinanceDbContext _context;

    public AccountBalanceService(FinanceDbContext context)
    {
        _context = context;
    }

    public async Task<decimal> GetAccountBalanceAsync(
        int accountId,
        int userId)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a =>
                a.Id == accountId &&
                a.UserId == userId &&
                a.IsActive);

        if (account == null)
        {
            throw new InvalidOperationException(
                "Account not found.");
        }

        if (account.AccountType == "CREDIT_CARD")
        {
            var purchases = await _context.Transactions
                .Where(t =>
                    t.UserId == userId &&
                    t.AccountId == accountId &&
                    t.Purpose ==
                        TransactionPurpose.CreditCardPurchase)
                .SumAsync(t => (decimal?)t.Amount) ?? 0;

            var payments = await _context.Transactions
                .Where(t =>
                    t.UserId == userId &&
                    t.ToAccountId == accountId &&
                    t.Purpose ==
                        TransactionPurpose.CreditCardPayment)
                .SumAsync(t => (decimal?)t.Amount) ?? 0;

            return Math.Max(
                purchases - payments,
                0);
        }

        var income = await _context.Transactions
    .Where(t =>
        t.UserId == userId &&
        t.AccountId == accountId &&
        t.Type == TransactionType.Income &&
        t.Purpose != TransactionPurpose.Deposit &&
        t.Purpose != TransactionPurpose.LoanBorrowed &&
        t.Purpose != TransactionPurpose.LoanReceived &&
        t.Purpose != TransactionPurpose.InvestmentSale)
    .SumAsync(t => (decimal?)t.Amount) ?? 0;

        var deposits = await _context.Transactions
            .Where(t =>
                t.UserId == userId &&
                t.AccountId == accountId &&
                t.Purpose == TransactionPurpose.Deposit)
            .SumAsync(t => (decimal?)t.Amount) ?? 0;

        var expenses = await _context.Transactions
            .Where(t =>
                t.UserId == userId &&
                t.AccountId == accountId &&
                t.Type == TransactionType.Expense &&
                t.Purpose != TransactionPurpose.Investment)
            .SumAsync(t => (decimal?)t.Amount) ?? 0;

        var investments = await _context.Transactions
            .Where(t =>
                t.UserId == userId &&
                t.AccountId == accountId &&
                t.Purpose == TransactionPurpose.Investment)
            .SumAsync(t => (decimal?)t.Amount) ?? 0;

        var transfersIn = await _context.Transactions
            .Where(t =>
                t.UserId == userId &&
                t.ToAccountId == accountId &&
                t.Type == TransactionType.Transfer)
            .SumAsync(t => (decimal?)t.Amount) ?? 0;

        var transfersOut = await _context.Transactions
            .Where(t =>
                t.UserId == userId &&
                t.FromAccountId == accountId &&
                t.Type == TransactionType.Transfer)
            .SumAsync(t => (decimal?)t.Amount) ?? 0;

        var creditCardPayments = await _context.Transactions
            .Where(t =>
                t.UserId == userId &&
                t.FromAccountId == accountId &&
                t.Purpose ==
                    TransactionPurpose.CreditCardPayment)
            .SumAsync(t => (decimal?)t.Amount) ?? 0;
        var investmentSales = await _context.Transactions
    .Where(t =>
        t.UserId == userId &&
        t.AccountId == accountId &&
        t.Purpose == TransactionPurpose.InvestmentSale)
    .SumAsync(t => (decimal?)t.Amount) ?? 0;
        return
     account.OpeningBalance
     + income
     + deposits
     + investmentSales
     - expenses
     - investments
     + transfersIn
     - transfersOut
     - creditCardPayments;
    }
}