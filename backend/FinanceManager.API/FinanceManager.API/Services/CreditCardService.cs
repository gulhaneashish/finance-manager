using FinanceManager.API.Data;
using FinanceManager.API.DTOs;
using FinanceManager.API.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.API.Services;

public class CreditCardService
{
    private readonly FinanceDbContext _context;

    public CreditCardService(FinanceDbContext context)
    {
        _context = context;
    }


    // =========================================================
    // CREDIT CARD PURCHASE
    // =========================================================

    public async Task<Transaction> MakePurchaseAsync(
        CreditCardPurchaseDto dto,
        int userId)
    {
        var card = await _context.Accounts
            .FirstOrDefaultAsync(a =>
                a.Id == dto.AccountId &&
                a.UserId == userId &&
                a.IsActive);

        if (card == null)
        {
            throw new InvalidOperationException(
                "Credit card account not found.");
        }

        if (card.AccountType != "CREDIT_CARD")
        {
            throw new InvalidOperationException(
                "Selected account is not a credit card.");
        }

        if (!card.CreditLimit.HasValue ||
            card.CreditLimit.Value <= 0)
        {
            throw new InvalidOperationException(
                "Credit card limit is not configured.");
        }

        if (dto.Amount <= 0)
        {
            throw new InvalidOperationException(
                "Purchase amount must be greater than zero.");
        }


        // =====================================================
        // VALIDATE CATEGORY
        // =====================================================

        var category = await _context.Categories
            .FirstOrDefaultAsync(c =>
                c.Id == dto.CategoryId &&
                c.UserId == userId &&
                c.Type == "EXPENSE");

        if (category == null)
        {
            throw new InvalidOperationException(
                "Valid expense category not found.");
        }


        // =====================================================
        // CURRENT OUTSTANDING
        // =====================================================

        var outstanding = await GetOutstandingAsync(
            card.Id,
            userId);


        // =====================================================
        // AVAILABLE CREDIT
        // =====================================================

        var availableCredit =
            card.CreditLimit.Value - outstanding;

        if (dto.Amount > availableCredit)
        {
            throw new InvalidOperationException(
                "Credit limit exceeded.");
        }


        // =====================================================
        // CREATE PURCHASE
        // =====================================================

        var transaction = new Transaction
        {
            UserId = userId,

            AccountId = card.Id,

            CategoryId = dto.CategoryId,

            Amount = dto.Amount,

            Type = TransactionType.CreditCard,

            Purpose =
                TransactionPurpose.CreditCardPurchase,

            Description =
                string.IsNullOrWhiteSpace(dto.Description)
                    ? "Credit card purchase"
                    : dto.Description,

            TransactionDate = dto.TransactionDate,

            CreatedAt = DateTime.UtcNow
        };

        _context.Transactions.Add(transaction);

        await _context.SaveChangesAsync();

        return transaction;
    }


    // =========================================================
    // GET CREDIT CARD OUTSTANDING
    // =========================================================

    public async Task<decimal> GetOutstandingAsync(
        int creditCardAccountId,
        int userId)
    {
        var card = await _context.Accounts
            .FirstOrDefaultAsync(a =>
                a.Id == creditCardAccountId &&
                a.UserId == userId &&
                a.AccountType == "CREDIT_CARD");

        if (card == null)
        {
            throw new InvalidOperationException(
                "Credit card not found.");
        }


        // -----------------------------------------------------
        // TOTAL PURCHASES
        // -----------------------------------------------------

        var purchases = await _context.Transactions
            .Where(t =>
                t.UserId == userId &&
                t.AccountId == creditCardAccountId &&
                t.Purpose ==
                    TransactionPurpose.CreditCardPurchase)
            .SumAsync(t =>
                (decimal?)t.Amount) ?? 0;


        // -----------------------------------------------------
        // TOTAL PAYMENTS
        // -----------------------------------------------------

        var payments = await _context.Transactions
            .Where(t =>
                t.UserId == userId &&
                t.ToAccountId == creditCardAccountId &&
                t.Purpose ==
                    TransactionPurpose.CreditCardPayment)
            .SumAsync(t =>
                (decimal?)t.Amount) ?? 0;


        // -----------------------------------------------------
        // OUTSTANDING
        // -----------------------------------------------------

        return Math.Max(
            purchases - payments,
            0);
    }


    // =========================================================
    // CREDIT CARD PAYMENT
    //
    // NORMAL ACCOUNT
    //       ↓
    // CREDIT CARD
    // =========================================================

    public async Task<Transaction> MakePaymentAsync(
        CreditCardPaymentDto dto,
        int userId)
    {
        if (dto.Amount <= 0)
        {
            throw new InvalidOperationException(
                "Payment amount must be greater than zero.");
        }


        // =====================================================
        // SOURCE ACCOUNT
        // =====================================================

        var fromAccount = await _context.Accounts
            .FirstOrDefaultAsync(a =>
                a.Id == dto.FromAccountId &&
                a.UserId == userId &&
                a.IsActive);

        if (fromAccount == null)
        {
            throw new InvalidOperationException(
                "Source account not found.");
        }


        if (fromAccount.AccountType == "CREDIT_CARD")
        {
            throw new InvalidOperationException(
                "Source account cannot be a credit card.");
        }


        // =====================================================
        // CREDIT CARD
        // =====================================================

        var card = await _context.Accounts
            .FirstOrDefaultAsync(a =>
                a.Id == dto.CreditCardAccountId &&
                a.UserId == userId &&
                a.IsActive);

        if (card == null)
        {
            throw new InvalidOperationException(
                "Credit card account not found.");
        }


        if (card.AccountType != "CREDIT_CARD")
        {
            throw new InvalidOperationException(
                "Destination account must be a credit card.");
        }


        // =====================================================
        // SAME ACCOUNT CHECK
        // =====================================================

        if (fromAccount.Id == card.Id)
        {
            throw new InvalidOperationException(
                "Source account and credit card cannot be the same.");
        }


        // =====================================================
        // CURRENT CARD OUTSTANDING
        // =====================================================

        var outstanding = await GetOutstandingAsync(
            card.Id,
            userId);

        if (outstanding <= 0)
        {
            throw new InvalidOperationException(
                "Credit card has no outstanding amount.");
        }


        if (dto.Amount > outstanding)
        {
            throw new InvalidOperationException(
                "Payment cannot exceed credit card outstanding.");
        }


        // =====================================================
        // SOURCE ACCOUNT BALANCE
        // =====================================================

        var sourceBalance = await GetAccountBalanceAsync(
            fromAccount.Id,
            userId);

        if (dto.Amount > sourceBalance)
        {
            throw new InvalidOperationException(
                $"Insufficient balance. Available balance: ₹{sourceBalance}");
        }


        // =====================================================
        // CREATE PAYMENT TRANSACTION
        // =====================================================

        var transaction = new Transaction
        {
            UserId = userId,

            FromAccountId = fromAccount.Id,

            ToAccountId = card.Id,

            Amount = dto.Amount,

            Type = TransactionType.Settlement,

            Purpose =
                TransactionPurpose.CreditCardPayment,

            Description =
                string.IsNullOrWhiteSpace(dto.Description)
                    ? "Credit card payment"
                    : dto.Description,

            TransactionDate = dto.PaymentDate,

            CreatedAt = DateTime.UtcNow
        };

        _context.Transactions.Add(transaction);

        await _context.SaveChangesAsync();

        return transaction;
    }


    // =========================================================
    // SOURCE ACCOUNT BALANCE
    // =========================================================

    private async Task<decimal> GetAccountBalanceAsync(
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


        // =====================================================
        // INCOME
        // =====================================================

        var income = await _context.Transactions
            .Where(t =>
                t.UserId == userId &&
                t.AccountId == accountId &&
                t.Type == TransactionType.Income &&
                t.Purpose != TransactionPurpose.Deposit)
            .SumAsync(t =>
                (decimal?)t.Amount) ?? 0;


        // =====================================================
        // DEPOSITS
        // =====================================================

        var deposits = await _context.Transactions
            .Where(t =>
                t.UserId == userId &&
                t.AccountId == accountId &&
                t.Purpose ==
                    TransactionPurpose.Deposit)
            .SumAsync(t =>
                (decimal?)t.Amount) ?? 0;


        // =====================================================
        // EXPENSES
        // =====================================================

        var expenses = await _context.Transactions
            .Where(t =>
                t.UserId == userId &&
                t.AccountId == accountId &&
                t.Type == TransactionType.Expense)
            .SumAsync(t =>
                (decimal?)t.Amount) ?? 0;


        // =====================================================
        // TRANSFERS IN
        // =====================================================

        var transfersIn = await _context.Transactions
            .Where(t =>
                t.UserId == userId &&
                t.ToAccountId == accountId &&
                t.Type == TransactionType.Transfer)
            .SumAsync(t =>
                (decimal?)t.Amount) ?? 0;


        // =====================================================
        // TRANSFERS OUT
        // =====================================================

        var transfersOut = await _context.Transactions
            .Where(t =>
                t.UserId == userId &&
                t.FromAccountId == accountId &&
                t.Type == TransactionType.Transfer)
            .SumAsync(t =>
                (decimal?)t.Amount) ?? 0;


        // =====================================================
        // CREDIT CARD PAYMENTS
        // =====================================================

        var creditCardPayments = await _context.Transactions
            .Where(t =>
                t.UserId == userId &&
                t.FromAccountId == accountId &&
                t.Purpose ==
                    TransactionPurpose.CreditCardPayment)
            .SumAsync(t =>
                (decimal?)t.Amount) ?? 0;


        // =====================================================
        // FINAL BALANCE
        // =====================================================

        return account.OpeningBalance
            + income
            + deposits
            + transfersIn
            - expenses
            - transfersOut
            - creditCardPayments;
    }
}