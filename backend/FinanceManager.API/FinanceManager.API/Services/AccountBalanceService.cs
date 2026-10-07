using FinanceManager.API.Models;
using FinanceManager.API.Repositories.Interfaces;
using FinanceManager.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.API.Services;

public class AccountBalanceService : IAccountBalanceService
{
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionRepository _transactionRepository;

    public AccountBalanceService(
        IAccountRepository accountRepository,
        ITransactionRepository transactionRepository)
    {
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<decimal> GetAccountBalanceAsync(
        int accountId,
        int userId)
    {
        // ============================================================
        // GET ACCOUNT
        // ============================================================

        var account = await _accountRepository.FirstOrDefaultAsync(a =>
            a.Id == accountId &&
            a.UserId == userId);

        if (account == null)
        {
            throw new InvalidOperationException(
                "Account not found.");
        }


        // ============================================================
        // CREDIT CARD BALANCE
        // ============================================================

        if (account.AccountType == "CREDIT_CARD")
        {
            var purchases = await _transactionRepository.Query()
                .Where(t =>
                    t.UserId == userId &&
                    t.AccountId == accountId &&
                    t.Purpose == TransactionPurpose.CreditCardPurchase)
                .SumAsync(t => (decimal?)t.Amount) ?? 0;

            var payments = await _transactionRepository.Query()
                .Where(t =>
                    t.UserId == userId &&
                    t.ToAccountId == accountId &&
                    t.Purpose == TransactionPurpose.CreditCardPayment)
                .SumAsync(t => (decimal?)t.Amount) ?? 0;

            var outstanding = purchases - payments;

            return Math.Max(outstanding, 0);
        }


        // ============================================================
        // NORMAL ACCOUNT
        // ============================================================

        // Normal income.
        //
        // OpeningBalance transactions are NOT included because the opening balance
        // is already added directly via account.OpeningBalance.
        var income = await _transactionRepository.Query()
            .Where(t =>
                t.UserId == userId &&
                t.AccountId == accountId &&
                t.Type == TransactionType.Income &&
                t.Purpose != TransactionPurpose.Deposit &&
                t.Purpose != TransactionPurpose.OpeningBalance &&
                t.Description != "Opening balance" &&
                // t.Purpose != TransactionPurpose.LoanBorrowed &&
                // t.Purpose != TransactionPurpose.LoanReceived &&
                t.Purpose != TransactionPurpose.InvestmentSale)
            .SumAsync(t => (decimal?)t.Amount) ?? 0;


        // ============================================================
        // NORMAL DEPOSITS
        // ============================================================

        // This includes money added AFTER account creation.
        //
        // The initial OpeningBalance transaction is NOT a Deposit
        // anymore, so it will not be counted here.
        var deposits = await _transactionRepository.Query()
            .Where(t =>
                t.UserId == userId &&
                t.AccountId == accountId &&
                t.Purpose == TransactionPurpose.Deposit &&
                t.Purpose != TransactionPurpose.OpeningBalance &&
                t.Description != "Opening balance")
            .SumAsync(t => (decimal?)t.Amount) ?? 0;


        // ============================================================
        // EXPENSES
        // ============================================================

        var expenses = await _transactionRepository.Query()
            .Where(t =>
                t.UserId == userId &&
                t.AccountId == accountId &&
                t.Type == TransactionType.Expense &&
                t.Purpose != TransactionPurpose.Investment)
            .SumAsync(t => (decimal?)t.Amount) ?? 0;


        // ============================================================
        // INVESTMENTS
        // ============================================================

        var investments = await _transactionRepository.Query()
            .Where(t =>
                t.UserId == userId &&
                t.AccountId == accountId &&
                t.Purpose == TransactionPurpose.Investment)
            .SumAsync(t => (decimal?)t.Amount) ?? 0;


        // ============================================================
        // TRANSFERS IN
        // ============================================================

        var transfersIn = await _transactionRepository.Query()
            .Where(t =>
                t.UserId == userId &&
                t.ToAccountId == accountId &&
                t.Type == TransactionType.Transfer)
            .SumAsync(t => (decimal?)t.Amount) ?? 0;


        // ============================================================
        // TRANSFERS OUT
        // ============================================================

        var transfersOut = await _transactionRepository.Query()
            .Where(t =>
                t.UserId == userId &&
                t.FromAccountId == accountId &&
                t.Type == TransactionType.Transfer)
            .SumAsync(t => (decimal?)t.Amount) ?? 0;


        // ============================================================
        // CREDIT CARD PAYMENTS
        // ============================================================

        var creditCardPayments = await _transactionRepository.Query()
            .Where(t =>
                t.UserId == userId &&
                t.FromAccountId == accountId &&
                t.Purpose == TransactionPurpose.CreditCardPayment)
            .SumAsync(t => (decimal?)t.Amount) ?? 0;


        // ============================================================
        // INVESTMENT SALES
        // ============================================================

        var investmentSales = await _transactionRepository.Query()
            .Where(t =>
                t.UserId == userId &&
                t.AccountId == accountId &&
                t.Purpose == TransactionPurpose.InvestmentSale)
            .SumAsync(t => (decimal?)t.Amount) ?? 0;


        // ============================================================
        // FINAL BALANCE
        // ============================================================

        var currentBalance =
            account.OpeningBalance
            + income
            + deposits
            + investmentSales
            - expenses
            - investments
            + transfersIn
            - transfersOut
            - creditCardPayments;

        return currentBalance;
    }
}