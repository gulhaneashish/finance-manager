using FinanceManager.API.DTOs;
using FinanceManager.API.Models;
using FinanceManager.API.Repositories.Interfaces;
using FinanceManager.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.API.Services;

public class NetWorthService : INetWorthService
{
    private readonly IAccountRepository _accountRepository;
    private readonly IInvestmentRepository _investmentRepository;
    private readonly ILoanRepository _loanRepository;
    private readonly IAccountBalanceService _accountBalanceService;

    public NetWorthService(
        IAccountRepository accountRepository,
        IInvestmentRepository investmentRepository,
        ILoanRepository loanRepository,
        IAccountBalanceService accountBalanceService)
    {
        _accountRepository = accountRepository;
        _investmentRepository = investmentRepository;
        _loanRepository = loanRepository;
        _accountBalanceService = accountBalanceService;
    }

    public async Task<NetWorthDto> GetNetWorthAsync(
        int userId)
    {
        var accounts = await _accountRepository.Query()
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
            await _investmentRepository.Query()
                .Where(i =>
                    i.UserId == userId &&
                    i.IsActive)
                .SumAsync(i =>
                    (decimal?)i.CurrentValue) ?? 0;

        var loans = await _loanRepository.Query()
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