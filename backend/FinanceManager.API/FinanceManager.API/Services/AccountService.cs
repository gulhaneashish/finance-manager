using FinanceManager.API.Common;
using FinanceManager.API.DTOs;
using FinanceManager.API.Models;
using FinanceManager.API.Repositories.Interfaces;
using FinanceManager.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.API.Services;

public class AccountService : IAccountService
{
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAccountBalanceService _accountBalanceService;
    public AccountService(
        IAccountRepository accountRepository,
        ITransactionRepository transactionRepository,
        IAccountBalanceService accountBalanceService)
    {
        _accountRepository = accountRepository;
        _transactionRepository = transactionRepository;
        _accountBalanceService = accountBalanceService;
    }

    // ============================================================
    // CREATE ACCOUNT
    // ============================================================

    public async Task<AccountResponseDto> CreateAsync(
        AccountCreateDto dto,
        int userId)
    {
        var allowedTypes = new[]
        {
            "BANK",
            "CASH",
            "SAVINGS",
            "INVESTMENT",
            "CREDIT_CARD"
        };

        var accountType = dto.AccountType
            .Trim()
            .ToUpper();

        // --------------------------------------------------------
        // Validate account type
        // --------------------------------------------------------

        if (!allowedTypes.Contains(accountType))
        {
            throw new InvalidOperationException(
                "Invalid account type.");
        }

        // --------------------------------------------------------
        // Validate account name
        // --------------------------------------------------------

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new InvalidOperationException(
                "Account name is required.");
        }

        // --------------------------------------------------------
        // Check duplicate active account
        // --------------------------------------------------------

        var accountName = dto.Name.Trim();

        var existingAccount = await _accountRepository.FirstOrDefaultAsync(a =>
            a.UserId == userId &&
            a.IsActive &&
            a.Name.ToLower() == accountName.ToLower() &&
            a.AccountType == accountType);

        if (existingAccount != null)
        {
            throw new InvalidOperationException(
                "An account with the same name and type already exists.");
        }

        // --------------------------------------------------------
        // Validate opening balance
        // --------------------------------------------------------

        if (dto.OpeningBalance < 0)
        {
            throw new InvalidOperationException(
                "Opening balance cannot be negative.");
        }

        // --------------------------------------------------------
        // Validate credit card
        // --------------------------------------------------------

        if (accountType == "CREDIT_CARD" &&
            (!dto.CreditLimit.HasValue ||
             dto.CreditLimit.Value <= 0))
        {
            throw new InvalidOperationException(
                "Credit card must have a valid credit limit.");
        }

        // --------------------------------------------------------
        // Credit limit not allowed for other accounts
        // --------------------------------------------------------

        if (accountType != "CREDIT_CARD" &&
            dto.CreditLimit.HasValue)
        {
            throw new InvalidOperationException(
                "Credit limit is only allowed for credit cards.");
        }

        // --------------------------------------------------------
        // Create Account
        // --------------------------------------------------------

        var account = new Account
        {
            UserId = userId,
            Name = accountName,
            AccountType = accountType,

            // This stores the original opening balance.
            OpeningBalance = dto.OpeningBalance,

            CreditLimit = dto.CreditLimit,

            IsActive = true
        };

        await _accountRepository.AddAsync(account);

        // Save first because we need Account.Id
        await _accountRepository.SaveChangesAsync();

        // --------------------------------------------------------
        // Create initial transaction
        // --------------------------------------------------------

        // Only create a transaction when opening balance > 0.
        if (dto.OpeningBalance > 0)
        {
            var initialTransaction = new Transaction
            {
                UserId = userId,

                AccountId = account.Id,

                Amount = dto.OpeningBalance,

                Type = TransactionType.Income,

                Purpose = TransactionPurpose.OpeningBalance,

                Description = "Opening balance",

                TransactionDate = DateTime.UtcNow,

                CreatedAt = DateTime.UtcNow
            };

            await _transactionRepository.AddAsync(initialTransaction);

            await _transactionRepository.SaveChangesAsync();
        }

        // --------------------------------------------------------
        // Return response
        // --------------------------------------------------------

        return new AccountResponseDto
        {
            Id = account.Id,
            Name = account.Name,
            AccountType = account.AccountType,
            OpeningBalance = account.OpeningBalance,
            CreditLimit = account.CreditLimit,

            CurrentBalance = await GetBalanceAsync(
                account.Id,
                userId) ?? 0,

            IsActive = account.IsActive
        };
    }


    // ============================================================
    // GET ALL ACCOUNTS
    // ============================================================

    public async Task<List<AccountResponseDto>> GetAllAsync(
        int userId)
    {
        // IMPORTANT:
        // Do NOT filter IsActive here.
        //
        // This will return:
        // Active accounts
        // + Inactive accounts
        //

        var accounts = await _accountRepository.Query()
            .Where(a =>
                a.UserId == userId)
            .OrderByDescending(a => a.IsActive)
            .ThenBy(a => a.Name)
            .ToListAsync();

        var result = new List<AccountResponseDto>();

        foreach (var account in accounts)
        {
            var balance = await GetBalanceAsync(
                account.Id,
                userId);

            result.Add(new AccountResponseDto
            {
                Id = account.Id,

                Name = account.Name,

                AccountType = account.AccountType,

                OpeningBalance = account.OpeningBalance,

                CreditLimit = account.CreditLimit,

                CurrentBalance = balance ?? 0,

                IsActive = account.IsActive
            });
        }

        return result;
    }


    // ============================================================
    // GET ACCOUNT BY ID
    // ============================================================

    public async Task<AccountResponseDto?> GetByIdAsync(
        int id,
        int userId)
    {
        var account = await _accountRepository.FirstOrDefaultAsync(a =>
            a.Id == id &&
            a.UserId == userId);

        if (account == null)
        {
            return null;
        }

        var balance = await GetBalanceAsync(
            account.Id,
            userId);

        return new AccountResponseDto
        {
            Id = account.Id,

            Name = account.Name,

            AccountType = account.AccountType,

            OpeningBalance = account.OpeningBalance,

            CreditLimit = account.CreditLimit,

            CurrentBalance = balance ?? 0,

            IsActive = account.IsActive
        };
    }


    // ============================================================
    // DEACTIVATE ACCOUNT
    // ============================================================

    public async Task<bool> DeleteAsync(
        int id,
        int userId)
    {
        var account = await _accountRepository.FirstOrDefaultAsync(a =>
            a.Id == id &&
            a.UserId == userId);

        if (account == null)
        {
            return false;
        }

        // Soft delete
        // Account remains in database.
        account.IsActive = false;

        await _accountRepository.SaveChangesAsync();

        return true;
    }


    // ============================================================
    // GET ACCOUNT BALANCE
    // ============================================================

    // public async Task<decimal?> GetBalanceAsync(
    //  int accountId,
    //  int userId)
    // {
    //     var account = await _accountRepository.FirstOrDefaultAsync(a =>
    //         a.Id == accountId &&
    //         a.UserId == userId);

    //     if (account == null)
    //     {
    //         return null;
    //     }

    //     // ========================================================
    //     // CREDIT CARD CALCULATION
    //     // ========================================================

    //     if (account.AccountType == "CREDIT_CARD")
    //     {
    //         var purchases = await _transactionRepository.Query()
    //             .Where(t =>
    //                 t.UserId == userId &&
    //                 t.AccountId == accountId &&
    //                 t.Purpose ==
    //                     TransactionPurpose.CreditCardPurchase)
    //             .SumAsync(t => (decimal?)t.Amount) ?? 0;

    //         var payments = await _transactionRepository.Query()
    //             .Where(t =>
    //                 t.UserId == userId &&
    //                 t.ToAccountId == accountId &&
    //                 t.Purpose ==
    //                     TransactionPurpose.CreditCardPayment)
    //             .SumAsync(t => (decimal?)t.Amount) ?? 0;

    //         var outstanding = purchases - payments;

    //         return Math.Max(outstanding, 0);
    //     }

    //     // ========================================================
    //     // NORMAL ACCOUNT CALCULATION
    //     // ========================================================

    //     var income = await _transactionRepository.Query()
    //         .Where(t =>
    //             t.UserId == userId &&
    //             t.AccountId == accountId &&
    //             t.Type == TransactionType.Income &&
    //             t.Purpose != TransactionPurpose.Deposit)
    //         .SumAsync(t => (decimal?)t.Amount) ?? 0;

    //     var deposits = await _transactionRepository.Query()
    //         .Where(t =>
    //             t.UserId == userId &&
    //             t.AccountId == accountId &&
    //             t.Purpose == TransactionPurpose.Deposit)
    //         .SumAsync(t => (decimal?)t.Amount) ?? 0;

    //     var expenses = await _transactionRepository.Query()
    //         .Where(t =>
    //             t.UserId == userId &&
    //             t.AccountId == accountId &&
    //             t.Type == TransactionType.Expense)
    //         .SumAsync(t => (decimal?)t.Amount) ?? 0;

    //     var transfersIn = await _transactionRepository.Query()
    //         .Where(t =>
    //             t.UserId == userId &&
    //             t.ToAccountId == accountId &&
    //             t.Type == TransactionType.Transfer)
    //         .SumAsync(t => (decimal?)t.Amount) ?? 0;

    //     var transfersOut = await _transactionRepository.Query()
    //         .Where(t =>
    //             t.UserId == userId &&
    //             t.FromAccountId == accountId &&
    //             t.Type == TransactionType.Transfer)
    //         .SumAsync(t => (decimal?)t.Amount) ?? 0;

    //     var creditCardPayments = await _transactionRepository.Query()
    //         .Where(t =>
    //             t.UserId == userId &&
    //             t.FromAccountId == accountId &&
    //             t.Purpose ==
    //                 TransactionPurpose.CreditCardPayment)
    //         .SumAsync(t => (decimal?)t.Amount) ?? 0;

    //     var currentBalance =
    //         account.OpeningBalance
    //         + income
    //         + deposits
    //         - expenses
    //         + transfersIn
    //         - transfersOut
    //         - creditCardPayments;

    //     return currentBalance;
    // }
    public async Task<decimal?> GetBalanceAsync(
    int accountId,
    int userId)
{
    var account = await _accountRepository.FirstOrDefaultAsync(a =>
        a.Id == accountId &&
        a.UserId == userId);

    if (account == null)
    {
        return null;
    }

    var balance = await _accountBalanceService
        .GetAccountBalanceAsync(accountId, userId);

    return balance;
}


    // ============================================================
    // UPDATE ACCOUNT
    // ============================================================

    public async Task<AccountResponseDto?> UpdateAsync(
        int id,
        AccountUpdateDto dto,
        int userId)
    {
        var account = await _accountRepository.FirstOrDefaultAsync(a =>
            a.Id == id &&
            a.UserId == userId);

        if (account == null)
        {
            return null;
        }

        var allowedTypes = new[]
        {
            "BANK",
            "CASH",
            "SAVINGS",
            "INVESTMENT",
            "CREDIT_CARD"
        };

        var accountType = dto.AccountType
            .Trim()
            .ToUpper();

        // --------------------------------------------------------
        // Validate account type
        // --------------------------------------------------------

        if (!allowedTypes.Contains(accountType))
        {
            throw new InvalidOperationException(
                "Invalid account type.");
        }

        // --------------------------------------------------------
        // Validate account name
        // --------------------------------------------------------

        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            throw new InvalidOperationException(
                "Account name is required.");
        }

        // --------------------------------------------------------
        // Validate credit card
        // --------------------------------------------------------

        if (accountType == "CREDIT_CARD" &&
            (!dto.CreditLimit.HasValue ||
             dto.CreditLimit.Value <= 0))
        {
            throw new InvalidOperationException(
                "Credit card must have a valid credit limit.");
        }

        // --------------------------------------------------------
        // Validate non-credit account
        // --------------------------------------------------------

        if (accountType != "CREDIT_CARD" &&
            dto.CreditLimit.HasValue)
        {
            throw new InvalidOperationException(
                "Credit limit is only allowed for credit cards.");
        }

        account.Name = dto.Name.Trim();

        account.AccountType = accountType;

        account.CreditLimit = dto.CreditLimit;

        account.IsActive = dto.IsActive;

        await _accountRepository.SaveChangesAsync();

        var balance = await GetBalanceAsync(
            account.Id,
            userId);

        return new AccountResponseDto
        {
            Id = account.Id,

            Name = account.Name,

            AccountType = account.AccountType,

            OpeningBalance = account.OpeningBalance,

            CreditLimit = account.CreditLimit,

            CurrentBalance = balance ?? 0,

            IsActive = account.IsActive
        };
    }


    // ============================================================
    // ADD MONEY / DEPOSIT
    // ============================================================

    public async Task<Transaction> AddMoneyAsync(
        AddMoneyDto dto,
        int userId)
    {
        if (dto.Amount <= 0)
        {
            throw new InvalidOperationException(
                "Amount must be greater than zero.");
        }

        var account = await _accountRepository.FirstOrDefaultAsync(a =>
            a.Id == dto.AccountId &&
            a.UserId == userId &&
            a.IsActive);

        if (account == null)
        {
            throw new InvalidOperationException(
                "Active account not found.");
        }

        // --------------------------------------------------------
        // Create transaction
        // --------------------------------------------------------

        var transaction = new Transaction
        {
            UserId = userId,

            AccountId = account.Id,

            Amount = dto.Amount,

            Type = TransactionType.Income,

            Purpose = TransactionPurpose.Deposit,

            Description =
                string.IsNullOrWhiteSpace(dto.Description)
                    ? "Money deposited"
                    : dto.Description,

            TransactionDate =
                dto.TransactionDate.ToUniversalUtc(),

            CreatedAt =
                DateTime.UtcNow
        };

        await _transactionRepository.AddAsync(transaction);

        await _transactionRepository.SaveChangesAsync();

        return transaction;
    
    }

    public async Task<List<AccountResponseDto>> GetActiveAccountsAsync(
    int userId)
    {
        var accounts = await _accountRepository.Query()
            .Where(a =>
                a.UserId == userId &&
                a.IsActive)
            .OrderBy(a => a.Name)
            .ToListAsync();

        var result = new List<AccountResponseDto>();

        foreach (var account in accounts)
        {
            var balance = await GetBalanceAsync(
                account.Id,
                userId);

            result.Add(new AccountResponseDto
            {
                Id = account.Id,
                Name = account.Name,
                AccountType = account.AccountType,
                OpeningBalance = account.OpeningBalance,
                CreditLimit = account.CreditLimit,
                CurrentBalance = balance ?? 0,
                IsActive = account.IsActive
            });
        }

        return result;
    }
}