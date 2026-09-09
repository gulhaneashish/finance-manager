using FinanceManager.API.Data;
using FinanceManager.API.DTOs;
using FinanceManager.API.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.API.Services;

public class AccountService
{
    private readonly FinanceDbContext _context;

    public AccountService(FinanceDbContext context)
    {
        _context = context;
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

        var existingAccount = await _context.Accounts
            .FirstOrDefaultAsync(a =>
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

        _context.Accounts.Add(account);

        // Save first because we need Account.Id
        await _context.SaveChangesAsync();

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

                Purpose = TransactionPurpose.Deposit,

                Description = "Opening balance",

                TransactionDate = DateTime.UtcNow,

                CreatedAt = DateTime.UtcNow
            };

            _context.Transactions.Add(initialTransaction);

            await _context.SaveChangesAsync();
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

        var accounts = await _context.Accounts
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
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a =>
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
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a =>
                a.Id == id &&
                a.UserId == userId);

        if (account == null)
        {
            return false;
        }

        // Soft delete
        // Account remains in database.
        account.IsActive = false;

        await _context.SaveChangesAsync();

        return true;
    }


    // ============================================================
    // GET ACCOUNT BALANCE
    // ============================================================

    public async Task<decimal?> GetBalanceAsync(
     int accountId,
     int userId)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a =>
                a.Id == accountId &&
                a.UserId == userId);

        if (account == null)
        {
            return null;
        }

        // --------------------------------------------------------
        // CREDIT CARD
        // --------------------------------------------------------

        if (account.AccountType == "CREDIT_CARD")
        {
            var purchases = await _context.Transactions
                .Where(t =>
                    t.UserId == userId &&
                    t.AccountId == accountId &&
                    t.Purpose ==
                        TransactionPurpose.CreditCardPurchase)
                .SumAsync(t =>
                    (decimal?)t.Amount) ?? 0;

            var payments = await _context.Transactions
                .Where(t =>
                    t.UserId == userId &&
                    t.ToAccountId == accountId &&
                    t.Purpose ==
                        TransactionPurpose.CreditCardPayment)
                .SumAsync(t =>
                    (decimal?)t.Amount) ?? 0;

            return purchases - payments;
        }

        // --------------------------------------------------------
        // NORMAL ACCOUNT
        // --------------------------------------------------------

        var income = await _context.Transactions
            .Where(t =>
                t.AccountId == accountId &&
                t.UserId == userId &&
                t.Type == TransactionType.Income &&
                t.Purpose != TransactionPurpose.Deposit)
            .SumAsync(t =>
                (decimal?)t.Amount) ?? 0;

        var deposits = await _context.Transactions
            .Where(t =>
                t.AccountId == accountId &&
                t.UserId == userId &&
                t.Purpose == TransactionPurpose.Deposit)
            .SumAsync(t =>
                (decimal?)t.Amount) ?? 0;

        var expenses = await _context.Transactions
            .Where(t =>
                t.AccountId == accountId &&
                t.UserId == userId &&
                t.Type == TransactionType.Expense)
            .SumAsync(t =>
                (decimal?)t.Amount) ?? 0;

        var transfersIn = await _context.Transactions
            .Where(t =>
                t.ToAccountId == accountId &&
                t.UserId == userId &&
                t.Type == TransactionType.Transfer)
            .SumAsync(t =>
                (decimal?)t.Amount) ?? 0;

        var transfersOut = await _context.Transactions
            .Where(t =>
                t.FromAccountId == accountId &&
                t.UserId == userId &&
                t.Type == TransactionType.Transfer)
            .SumAsync(t =>
                (decimal?)t.Amount) ?? 0;

        var creditCardPayments = await _context.Transactions
            .Where(t =>
                t.FromAccountId == accountId &&
                t.UserId == userId &&
                t.Purpose ==
                    TransactionPurpose.CreditCardPayment)
            .SumAsync(t =>
                (decimal?)t.Amount) ?? 0;

        return account.OpeningBalance
            + income
            + deposits
            + transfersIn
            - expenses
            - transfersOut
            - creditCardPayments;
    }


    // ============================================================
    // UPDATE ACCOUNT
    // ============================================================

    public async Task<AccountResponseDto?> UpdateAsync(
        int id,
        AccountUpdateDto dto,
        int userId)
    {
        var account = await _context.Accounts
            .FirstOrDefaultAsync(a =>
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

        // --------------------------------------------------------
        // IMPORTANT:
        // Do NOT update OpeningBalance here.
        //
        // OpeningBalance was already recorded through the
        // initial transaction.
        //
        // If the user wants to add money later, use AddMoneyAsync().
        // --------------------------------------------------------

        account.Name = dto.Name.Trim();

        account.AccountType = accountType;

        account.CreditLimit = dto.CreditLimit;

        account.IsActive = dto.IsActive;

        await _context.SaveChangesAsync();

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

        var account = await _context.Accounts
            .FirstOrDefaultAsync(a =>
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
                dto.TransactionDate,

            CreatedAt =
                DateTime.UtcNow
        };

        _context.Transactions.Add(transaction);

        await _context.SaveChangesAsync();

        return transaction;
    
    }

    public async Task<List<AccountResponseDto>> GetActiveAccountsAsync(
    int userId)
    {
        var accounts = await _context.Accounts
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