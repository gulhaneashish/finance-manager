using FinanceManager.API.Data;
using FinanceManager.API.DTOs;
using FinanceManager.API.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.API.Services;

public class NetWorthService
{
    private readonly FinanceDbContext _context;
    private readonly AccountBalanceService _accountBalanceService;

    public NetWorthService(
        FinanceDbContext context,
        AccountBalanceService accountBalanceService)
    {
        _context = context;
        _accountBalanceService = accountBalanceService;
    }

    public async Task<NetWorthDto> GetNetWorthAsync(
        int userId)
    {
        var accounts = await _context.Accounts
            .Where(a =>
                a.UserId == userId &&
                a.IsActive)
            .ToListAsync();

        decimal bankBalance = 0;
        decimal cashBalance = 0;
        decimal savingsBalance = 0;
        decimal creditCardDebt = 0;

        foreach (var account in accounts)
        {
            var balance =
                await _accountBalanceService
                    .GetAccountBalanceAsync(
                        account.Id,
                        userId);

            switch (account.AccountType)
            {
                case "BANK":
                    bankBalance += balance;
                    break;

                case "CASH":
                    cashBalance += balance;
                    break;

                case "SAVINGS":
                    savingsBalance += balance;
                    break;

                case "CREDIT_CARD":
                    creditCardDebt += balance;
                    break;
            }
        }

        var investmentBalance =
            await _context.Investments
                .Where(i =>
                    i.UserId == userId &&
                    i.IsActive)
                .SumAsync(i =>
                    (decimal?)i.CurrentValue) ?? 0;

        var loans = await _context.Loans
            .Where(l =>
                l.UserId == userId &&
                l.IsActive)
            .Include(l => l.Payments)
            .ToListAsync();

        var loansPayable = loans
            .Where(l =>
                l.Type == LoanType.Borrowed)
            .Sum(l =>
                Math.Max(
                    l.OriginalAmount -
                    l.Payments.Sum(p => p.Amount),
                    0));

        var loansReceivable = loans
            .Where(l =>
                l.Type == LoanType.Lent)
            .Sum(l =>
                Math.Max(
                    l.OriginalAmount -
                    l.Payments.Sum(p => p.Amount),
                    0));

        var totalAssets =
            bankBalance +
            cashBalance +
            savingsBalance +
            investmentBalance +
            loansReceivable;

        var totalLiabilities =
            creditCardDebt +
            loansPayable;

        var netWorth =
            totalAssets -
            totalLiabilities;

        return new NetWorthDto
        {
            TotalAssets = totalAssets,

            BankBalance = bankBalance,

            CashBalance = cashBalance,

            SavingsBalance = savingsBalance,

            InvestmentBalance = investmentBalance,

            CreditCardDebt = creditCardDebt,

            LoansPayable = loansPayable,

            LoansReceivable = loansReceivable,

            TotalLiabilities = totalLiabilities,

            NetWorth = netWorth
        };
    }
}