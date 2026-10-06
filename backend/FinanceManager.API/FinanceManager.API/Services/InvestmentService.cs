using FinanceManager.API.Common;
using FinanceManager.API.DTOs;
using FinanceManager.API.Models;
using FinanceManager.API.Repositories.Interfaces;
using FinanceManager.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.API.Services;

public class InvestmentService : IInvestmentService
{
    private readonly IInvestmentRepository _investmentRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAccountBalanceService _accountBalanceService;

    public InvestmentService(
        IInvestmentRepository investmentRepository,
        IAccountRepository accountRepository,
        ITransactionRepository transactionRepository,
        IAccountBalanceService accountBalanceService)
    {
        _investmentRepository = investmentRepository;
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
        _accountBalanceService = accountBalanceService;
    }

    public async Task CreateInvestmentAsync(
        InvestmentCreateDto dto,
        int userId)
    {
        if (dto.Amount <= 0)
        {
            throw new InvalidOperationException(
                "Investment amount must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new InvalidOperationException(
                "Investment name is required.");
        }

        var account = await _accountRepository.FirstOrDefaultAsync(a =>
            a.Id == dto.AccountId &&
            a.UserId == userId &&
            a.IsActive);

        if (account == null)
        {
            throw new InvalidOperationException(
                "Invalid account.");
        }

        if (account.AccountType == "CREDIT_CARD")
        {
            throw new InvalidOperationException(
                "Credit card cannot be used for investments.");
        }

        var balance = await _accountBalanceService.GetAccountBalanceAsync(
            dto.AccountId,
            userId);

        if (balance < dto.Amount)
        {
            throw new InvalidOperationException(
                "Insufficient balance.");
        }

        await using var transaction =
            await _investmentRepository.BeginTransactionAsync();

        try
        {
            var transactionRecord = new Transaction
            {
                UserId = userId,
                AccountId = dto.AccountId,
                FromAccountId = null,
                ToAccountId = null,
                CategoryId = null,
                Amount = dto.Amount,
                Type = TransactionType.Expense,
                Purpose = TransactionPurpose.Investment,
                TransactionDate = dto.InvestmentDate.ToUniversalUtc(),
                Description = dto.Description ?? $"Investment in {dto.Name}",
                CreatedAt = DateTime.UtcNow
            };

            await _transactionRepository.AddAsync(transactionRecord);
            await _transactionRepository.SaveChangesAsync();

            var investment = new Investment
            {
                UserId = userId,
                Name = dto.Name,
                InvestmentType = dto.InvestmentType,
                InvestedAmount = dto.Amount,
                CurrentValue = dto.Amount,
                InvestmentDate = dto.InvestmentDate,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                TransactionId = transactionRecord.Id
            };

            await _investmentRepository.AddAsync(investment);
            await _investmentRepository.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<InvestmentDto>> GetInvestmentsAsync(
        int userId)
    {
        var investments = await _investmentRepository.Query()
            .Where(i =>
                i.UserId == userId &&
                i.IsActive)
            .OrderByDescending(i => i.InvestmentDate)
            .ToListAsync();

        return investments.Select(i =>
        {
            var profitLoss =
                i.CurrentValue - i.InvestedAmount;

            var profitLossPercentage =
                i.InvestedAmount > 0
                    ? (profitLoss / i.InvestedAmount) * 100
                    : 0;

            return new InvestmentDto
            {
                Id = i.Id,
                Name = i.Name,
                InvestmentType = i.InvestmentType,
                InvestedAmount = i.InvestedAmount,
                CurrentValue = i.CurrentValue,
                ProfitLoss = profitLoss,
                ProfitLossPercentage = profitLossPercentage,
                InvestmentDate = i.InvestmentDate,
                IsActive = i.IsActive
            };
        }).ToList();
    }

    public async Task<InvestmentSummaryDto> GetSummaryAsync(
        int userId)
    {
        var investments = await _investmentRepository.Query()
            .Where(i =>
                i.UserId == userId &&
                i.IsActive)
            .ToListAsync();

        var totalInvested =
            investments.Sum(i => i.InvestedAmount);

        var totalCurrentValue =
            investments.Sum(i => i.CurrentValue);

        var totalProfitLoss =
            totalCurrentValue - totalInvested;

        var totalProfitLossPercentage =
            totalInvested > 0
                ? (totalProfitLoss / totalInvested) * 100
                : 0;

        return new InvestmentSummaryDto
        {
            TotalInvestedAmount = totalInvested,
            TotalCurrentValue = totalCurrentValue,
            TotalProfitLoss = totalProfitLoss,
            TotalProfitLossPercentage = totalProfitLossPercentage,
            InvestmentCount = investments.Count
        };
    }

    public async Task UpdateCurrentValueAsync(
        int investmentId,
        int userId,
        InvestmentUpdateDto dto)
    {
        if (dto.CurrentValue < 0)
        {
            throw new InvalidOperationException(
                "Current value cannot be negative.");
        }

        var investment = await _investmentRepository.FirstOrDefaultAsync(i =>
            i.Id == investmentId &&
            i.UserId == userId &&
            i.IsActive);

        if (investment == null)
        {
            throw new InvalidOperationException(
                "Investment not found.");
        }

        investment.CurrentValue = dto.CurrentValue;

        await _investmentRepository.SaveChangesAsync();
    }

    public async Task<string> DeleteInvestmentAsync(
        int userId,
        int investmentId)
    {
        var investment = await _investmentRepository.FirstOrDefaultAsync(i =>
            i.Id == investmentId &&
            i.UserId == userId &&
            i.IsActive);

        if (investment == null)
            throw new InvalidOperationException("Investment not found.");

        if (investment.TransactionId == null)
            throw new InvalidOperationException("Investment purchase transaction not found.");

        if (investment.CurrentValue != investment.InvestedAmount)
            throw new InvalidOperationException("Only investments with unchanged value can be cancelled.");

        var purchaseTransaction = await _transactionRepository.FirstOrDefaultAsync(t =>
            t.Id == investment.TransactionId &&
            t.UserId == userId);

        if (purchaseTransaction == null)
            throw new InvalidOperationException("Investment purchase transaction not found.");

        if (purchaseTransaction.AccountId == null)
            throw new InvalidOperationException("Investment source account not found.");

        await using var transaction =
            await _investmentRepository.BeginTransactionAsync();

        try
        {
            var reversalTransaction = new Transaction
            {
                UserId = userId,
                AccountId = purchaseTransaction.AccountId,
                Amount = investment.InvestedAmount,
                Type = TransactionType.Income,
                Purpose = TransactionPurpose.InvestmentSale,
                Description = $"Investment cancelled: {investment.Name}",
                TransactionDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            await _transactionRepository.AddAsync(reversalTransaction);

            investment.IsActive = false;
            investment.CurrentValue = 0;
            investment.InvestedAmount = 0;

            await _investmentRepository.SaveChangesAsync();

            await transaction.CommitAsync();

            return "Investment cancelled successfully.";
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<string> SellInvestmentAsync(
        int userId,
        int investmentId,
        InvestmentSellDto dto)
    {
        if (dto.SellAmount <= 0)
            throw new InvalidOperationException("Sell amount must be greater than zero.");

        var investment = await _investmentRepository.FirstOrDefaultAsync(i =>
            i.Id == investmentId &&
            i.UserId == userId &&
            i.IsActive);

        if (investment == null)
            throw new InvalidOperationException("Investment not found.");

        if (dto.SellAmount > investment.CurrentValue)
            throw new InvalidOperationException("Sell amount cannot exceed current investment value.");

        var account = await _accountRepository.FirstOrDefaultAsync(a =>
            a.Id == dto.AccountId &&
            a.UserId == userId &&
            a.IsActive);

        if (account == null)
            throw new InvalidOperationException("Account not found.");

        if (account.AccountType == "CREDIT_CARD")
            throw new InvalidOperationException("Investment sale cannot be deposited into a credit card.");

        await using var transaction =
            await _investmentRepository.BeginTransactionAsync();

        try
        {
            var soldPercentage = dto.SellAmount / investment.CurrentValue;
            var investedAmountSold = investment.InvestedAmount * soldPercentage;
            var remainingInvestedAmount = investment.InvestedAmount - investedAmountSold;
            var remainingCurrentValue = investment.CurrentValue - dto.SellAmount;

            var saleTransaction = new Transaction
            {
                UserId = userId,
                AccountId = dto.AccountId,
                Amount = dto.SellAmount,
                Type = TransactionType.Income,
                Purpose = TransactionPurpose.InvestmentSale,
                Description = dto.Description ?? $"Investment sale: {investment.Name}",
                TransactionDate = dto.SellDate.ToUniversalUtc(),
                CreatedAt = DateTime.UtcNow
            };

            await _transactionRepository.AddAsync(saleTransaction);

            if (remainingCurrentValue <= 0.01m)
            {
                investment.InvestedAmount = 0;
                investment.CurrentValue = 0;
                investment.IsActive = false;
            }
            else
            {
                investment.InvestedAmount = Math.Round(remainingInvestedAmount, 2);
                investment.CurrentValue = Math.Round(remainingCurrentValue, 2);
            }

            await _investmentRepository.SaveChangesAsync();

            await transaction.CommitAsync();

            return "Investment sold successfully.";
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}