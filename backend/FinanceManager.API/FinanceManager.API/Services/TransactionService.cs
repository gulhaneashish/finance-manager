//using FinanceManager.API.Data;
//using FinanceManager.API.DTOs;
//using FinanceManager.API.Models;
//using Microsoft.EntityFrameworkCore;

//namespace FinanceManager.API.Services;

//public class TransactionService
//{
//    private readonly FinanceDbContext _context;

//    public TransactionService(FinanceDbContext context)
//    {
//        _context = context;
//    }

//    public async Task<TransactionResponseDto> CreateAsync(
//     TransactionCreateDto dto,
//     int userId)
//    {
//        if (dto.Amount <= 0)
//        {
//            throw new InvalidOperationException(
//                "Transaction amount must be greater than zero.");
//        }
//        if (dto.Type != TransactionType.Income &&
//            dto.Type != TransactionType.Expense &&
//            dto.Type != TransactionType.Transfer)
//        {
//            throw new InvalidOperationException(
//                "Invalid transaction type.");
//        }

//        if (dto.Type == TransactionType.Transfer)
//        {
//            if (!dto.FromAccountId.HasValue ||
//                !dto.ToAccountId.HasValue)
//            {
//                throw new InvalidOperationException(
//                    "Transfer requires source and destination accounts.");
//            }

//            if (dto.FromAccountId == dto.ToAccountId)
//            {
//                throw new InvalidOperationException(
//                    "Source and destination accounts must be different.");
//            }

//            var accounts = await _context.Accounts
//                .Where(a =>
//                    (a.Id == dto.FromAccountId ||
//                     a.Id == dto.ToAccountId) &&
//                    a.UserId == userId)
//                .ToListAsync();

//            if (accounts.Count != 2)
//            {
//                throw new InvalidOperationException(
//                    "Invalid source or destination account.");
//            }
//        }
//        else
//        {
//            if (!dto.AccountId.HasValue)
//            {
//                throw new InvalidOperationException(
//                    "Account is required.");
//            }

//            var accountExists = await _context.Accounts
//                .AnyAsync(a =>
//                    a.Id == dto.AccountId &&
//                    a.UserId == userId);

//            if (!accountExists)
//            {
//                throw new InvalidOperationException(
//                    "Account does not belong to the current user.");
//            }
//        }

//        if (dto.CategoryId.HasValue)
//        {
//            var categoryExists = await _context.Categories
//                .AnyAsync(c =>
//                    c.Id == dto.CategoryId.Value &&
//                    c.UserId == userId);

//            if (!categoryExists)
//            {
//                throw new InvalidOperationException(
//                    "Category does not belong to the current user.");
//            }
//        }

//        var transaction = new Transaction
//        {
//            UserId = userId,
//            AccountId = dto.AccountId,
//            FromAccountId = dto.FromAccountId,
//            ToAccountId = dto.ToAccountId,
//            CategoryId = dto.CategoryId,
//            Amount = dto.Amount,
//            Type = dto.Type,

//            Purpose = dto.Type == TransactionType.Income
//            ? TransactionPurpose.Income
//            : TransactionPurpose.Expense,

//            Description = dto.Description,
//            TransactionDate = dto.TransactionDate,
//            CreatedAt = DateTime.UtcNow
//        };

//        _context.Transactions.Add(transaction);

//        await _context.SaveChangesAsync();

//        return MapToDto(transaction);
//    }

//    public async Task<List<TransactionResponseDto>> GetAllAsync(
//        int userId)
//    {
//        var transactions = await _context.Transactions
//            .Where(t => t.UserId == userId)
//            .OrderByDescending(t => t.TransactionDate)
//            .ToListAsync();

//        return transactions
//            .Select(MapToDto)
//            .ToList();
//    }

//    public async Task<TransactionResponseDto?> GetByIdAsync(
//        int id,
//        int userId)
//    {
//        var transaction = await _context.Transactions
//            .FirstOrDefaultAsync(t =>
//                t.Id == id &&
//                t.UserId == userId);

//        return transaction == null
//            ? null
//            : MapToDto(transaction);
//    }

//    public async Task<bool> DeleteAsync(
//        int id,
//        int userId)
//    {
//        var transaction = await _context.Transactions
//            .FirstOrDefaultAsync(t =>
//                t.Id == id &&
//                t.UserId == userId);

//        if (transaction == null)
//        {
//            return false;
//        }

//        _context.Transactions.Remove(transaction);

//        await _context.SaveChangesAsync();

//        return true;
//    }

//    private static TransactionResponseDto MapToDto(
//     Transaction transaction)
//    {
//        return new TransactionResponseDto
//        {
//            Id = transaction.Id,
//            AccountId = transaction.AccountId,
//            FromAccountId = transaction.FromAccountId,
//            ToAccountId = transaction.ToAccountId,
//            CategoryId = transaction.CategoryId,
//            Amount = transaction.Amount,
//            Type = transaction.Type,
//            Description = transaction.Description,
//            TransactionDate = transaction.TransactionDate,
//            CreatedAt = transaction.CreatedAt
//        };
//    }
//    public async Task CreateTransferAsync(
//    TransferCreateDto dto,
//    int userId)
//    {
//        if (dto.FromAccountId == dto.ToAccountId)
//        {
//            throw new InvalidOperationException(
//                "Source and destination accounts must be different.");
//        }

//        if (dto.Amount <= 0)
//        {
//            throw new InvalidOperationException(
//                "Transfer amount must be greater than zero.");
//        }


//        var fromAccount = await _context.Accounts
//            .FirstOrDefaultAsync(a =>
//                a.Id == dto.FromAccountId &&
//                a.UserId == userId &&
//                a.IsActive);

//        var toAccount = await _context.Accounts
//      .FirstOrDefaultAsync(a =>
//          a.Id == dto.ToAccountId &&
//          a.UserId == userId &&
//          a.IsActive);

//        if (toAccount == null)
//        {
//            throw new InvalidOperationException(
//                "Destination account not found.");
//        }

//        if (fromAccount.AccountType == "CREDIT_CARD")
//        {
//            throw new InvalidOperationException(
//                "Credit card cannot be used as a transfer source.");
//        }

//        if (toAccount.AccountType == "CREDIT_CARD")
//        {
//            throw new InvalidOperationException(
//                "You cannot transfer money directly to a credit card. Use Credit Card Payment instead.");
//        }
//        if (fromAccount == null ||
//            toAccount == null)
//        {
//            throw new InvalidOperationException(
//                "Invalid account.");
//        }

//        var balance = await GetAccountBalanceAsync(
//            dto.FromAccountId,
//            userId);

//        if (balance < dto.Amount)
//        {
//            throw new InvalidOperationException(
//                "Insufficient balance.");
//        }

//        await using var transaction =
//            await _context.Database.BeginTransactionAsync();

//        try
//        {
//            var transfer = new Transaction
//            {
//                UserId = userId,

//                AccountId = dto.FromAccountId,

//                FromAccountId = dto.FromAccountId,

//                ToAccountId = dto.ToAccountId,

//                Amount = dto.Amount,

//                Type = TransactionType.Transfer,

//                TransactionDate =
//                    dto.TransactionDate,

//                Description =
//                    dto.Description ??
//                    $"Transfer from {fromAccount.Name} to {toAccount.Name}",

//                CreatedAt = DateTime.UtcNow
//            };

//            _context.Transactions.Add(transfer);

//            await _context.SaveChangesAsync();

//            await transaction.CommitAsync();
//        }
//        catch
//        {
//            await transaction.RollbackAsync();
//            throw;
//        }
//    }
//    private async Task<decimal> GetAccountBalanceAsync(
//     int accountId,
//     int userId)
//    {
//        var account = await _context.Accounts
//            .FirstOrDefaultAsync(a =>
//                a.Id == accountId &&
//                a.UserId == userId &&
//                a.IsActive);

//        if (account == null)
//        {
//            throw new InvalidOperationException(
//                "Account not found.");
//        }

//        var transactions = await _context.Transactions
//            .Where(t =>
//                t.UserId == userId &&
//                (
//                    t.AccountId == accountId ||
//                    t.FromAccountId == accountId ||
//                    t.ToAccountId == accountId
//                ))
//            .ToListAsync();

//        decimal balance = account.OpeningBalance;

//        foreach (var transaction in transactions)
//        {
//            if (transaction.Type == TransactionType.Income)
//            {
//                balance += transaction.Amount;
//            }
//            else if (transaction.Type == TransactionType.Expense)
//            {
//                balance -= transaction.Amount;
//            }
//            else if (transaction.Type == TransactionType.Transfer)
//            {
//                if (transaction.FromAccountId == accountId)
//                {
//                    balance -= transaction.Amount;
//                }

//                if (transaction.ToAccountId == accountId)
//                {
//                    balance += transaction.Amount;
//                }
//            }
//        }

//        return balance;
//    }
//    public async Task<List<TransactionListDto>> GetFilteredAsync(
//    int userId,
//    TransactionFilterDto filter)
//    {
//        var query = _context.Transactions
//            .Include(t => t.Account)
//            .Include(t => t.Category)
//            .Where(t => t.UserId == userId)
//            .AsQueryable();

//        if (filter.FromDate.HasValue)
//        {
//            query = query.Where(t =>
//                t.TransactionDate >= filter.FromDate.Value);
//        }

//        if (filter.ToDate.HasValue)
//        {
//            var toDate = filter.ToDate.Value.Date.AddDays(1);

//            query = query.Where(t =>
//                t.TransactionDate < toDate);
//        }

//        if (filter.AccountId.HasValue)
//        {
//            query = query.Where(t =>
//                t.AccountId == filter.AccountId.Value);
//        }

//        if (filter.CategoryId.HasValue)
//        {
//            query = query.Where(t =>
//                t.CategoryId == filter.CategoryId.Value);
//        }

//        if (filter.Type.HasValue)
//        {
//            query = query.Where(t =>
//                t.Type == filter.Type.Value);
//        }

//        return await query
//            .OrderByDescending(t => t.TransactionDate)
//            .Select(t => new TransactionListDto
//            {
//                Id = t.Id,

//                AccountId = t.AccountId,

//                AccountName =
//                    t.Account != null
//                        ? t.Account.Name
//                        : null,

//                CategoryId = t.CategoryId,

//                CategoryName =
//                    t.Category != null
//                        ? t.Category.Name
//                        : null,

//                Amount = t.Amount,

//                Type = t.Type,

//                Description = t.Description,

//                TransactionDate =
//                    t.TransactionDate,

//                Purpose = t.Purpose
//            })
//            .ToListAsync();
//    }
//}

using FinanceManager.API.Data;
using FinanceManager.API.DTOs;
using FinanceManager.API.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.API.Services;

public class TransactionService
{
    private readonly FinanceDbContext _context;
    private readonly AccountBalanceService _accountBalanceService;
    public TransactionService(FinanceDbContext context, AccountBalanceService accountBalanceService)
    {
        _context = context;
        _accountBalanceService = accountBalanceService;
    }

    public async Task<TransactionResponseDto> CreateAsync(
        TransactionCreateDto dto,
        int userId)
    {
        if (dto.Amount <= 0)
        {
            throw new InvalidOperationException(
                "Transaction amount must be greater than zero.");
        }

        if (dto.Type != TransactionType.Income &&
            dto.Type != TransactionType.Expense &&
            dto.Type != TransactionType.Transfer)
        {
            throw new InvalidOperationException(
                "Invalid transaction type.");
        }

        if (dto.Type == TransactionType.Transfer)
        {
            if (!dto.FromAccountId.HasValue ||
                !dto.ToAccountId.HasValue)
            {
                throw new InvalidOperationException(
                    "Transfer requires source and destination accounts.");
            }

            if (dto.FromAccountId == dto.ToAccountId)
            {
                throw new InvalidOperationException(
                    "Source and destination accounts must be different.");
            }

            var accounts = await _context.Accounts
                .Where(a =>
                    (a.Id == dto.FromAccountId.Value ||
                     a.Id == dto.ToAccountId.Value) &&
                    a.UserId == userId &&
                    a.IsActive)
                .ToListAsync();

            if (accounts.Count != 2)
            {
                throw new InvalidOperationException(
                    "Invalid source or destination account.");
            }

            var fromAccount = accounts
                .First(a => a.Id == dto.FromAccountId.Value);

            var toAccount = accounts
                .First(a => a.Id == dto.ToAccountId.Value);

            if (fromAccount.AccountType == "CREDIT_CARD")
            {
                throw new InvalidOperationException(
                    "Credit card cannot be used as a transfer source.");
            }

            if (toAccount.AccountType == "CREDIT_CARD")
            {
                throw new InvalidOperationException(
                    "You cannot transfer money directly to a credit card. Use Credit Card Payment instead.");
            }

            var balance = await _accountBalanceService.GetAccountBalanceAsync(
                dto.FromAccountId.Value,
                userId);

            if (balance < dto.Amount)
            {
                throw new InvalidOperationException(
                    "Insufficient balance.");
            }
        }
        else
        {
            if (!dto.AccountId.HasValue)
            {
                throw new InvalidOperationException(
                    "Account is required.");
            }

            var account = await _context.Accounts
                .FirstOrDefaultAsync(a =>
                    a.Id == dto.AccountId.Value &&
                    a.UserId == userId &&
                    a.IsActive);

            if (account == null)
            {
                throw new InvalidOperationException(
                    "Account not found or inactive.");
            }

            if (account.AccountType == "CREDIT_CARD")
            {
                throw new InvalidOperationException(
                    "Use Credit Card Purchase for credit card transactions.");
            }
        }

        if (dto.CategoryId.HasValue)
        {
            var categoryExists = await _context.Categories
                .AnyAsync(c =>
                    c.Id == dto.CategoryId.Value &&
                    c.UserId == userId);

            if (!categoryExists)
            {
                throw new InvalidOperationException(
                    "Category does not belong to the current user.");
            }
        }

        var purpose =
            dto.Type == TransactionType.Income
                ? TransactionPurpose.Income
                : TransactionPurpose.Expense;

        var transaction = new Transaction
        {
            UserId = userId,

            AccountId = dto.AccountId,

            FromAccountId = dto.FromAccountId,

            ToAccountId = dto.ToAccountId,

            CategoryId = dto.CategoryId,

            Amount = dto.Amount,

            Type = dto.Type,

            Purpose = purpose,

            Description = dto.Description,

            TransactionDate = dto.TransactionDate,

            CreatedAt = DateTime.UtcNow
        };

        _context.Transactions.Add(transaction);

        await _context.SaveChangesAsync();

        return MapToDto(transaction);
    }

    public async Task<List<TransactionResponseDto>> GetAllAsync(
        int userId)
    {
        var transactions = await _context.Transactions
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.TransactionDate)
            .ToListAsync();

        return transactions
            .Select(MapToDto)
            .ToList();
    }

    public async Task<TransactionResponseDto?> GetByIdAsync(
        int id,
        int userId)
    {
        var transaction = await _context.Transactions
            .FirstOrDefaultAsync(t =>
                t.Id == id &&
                t.UserId == userId);

        return transaction == null
            ? null
            : MapToDto(transaction);
    }

    public async Task<bool> DeleteAsync(
        int id,
        int userId)
    {
        var transaction = await _context.Transactions
            .FirstOrDefaultAsync(t =>
                t.Id == id &&
                t.UserId == userId);

        if (transaction == null)
        {
            return false;
        }

        _context.Transactions.Remove(transaction);

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task CreateTransferAsync(
     TransferCreateDto dto,
     int userId)
    {
        if (dto.FromAccountId == dto.ToAccountId)
        {
            throw new InvalidOperationException(
                "Source and destination accounts must be different.");
        }

        if (dto.Amount <= 0)
        {
            throw new InvalidOperationException(
                "Transfer amount must be greater than zero.");
        }

        var fromAccount = await _context.Accounts
            .FirstOrDefaultAsync(a =>
                a.Id == dto.FromAccountId &&
                a.UserId == userId &&
                a.IsActive);

        var toAccount = await _context.Accounts
            .FirstOrDefaultAsync(a =>
                a.Id == dto.ToAccountId &&
                a.UserId == userId &&
                a.IsActive);

        if (fromAccount == null || toAccount == null)
        {
            throw new InvalidOperationException(
                "Invalid account.");
        }

        if (fromAccount.AccountType == "CREDIT_CARD")
        {
            throw new InvalidOperationException(
                "Credit card cannot be used as a transfer source.");
        }

        if (toAccount.AccountType == "CREDIT_CARD")
        {
            throw new InvalidOperationException(
                "You cannot transfer money directly to a credit card. Use Credit Card Payment instead.");
        }

        if (dto.TransactionPurpose != TransactionPurpose.Transfer &&
            dto.TransactionPurpose != TransactionPurpose.Savings)
        {
            throw new InvalidOperationException(
                "Invalid transfer purpose.");
        }

        if (dto.TransactionPurpose == TransactionPurpose.Savings &&
            toAccount.AccountType != "SAVINGS")
        {
            throw new InvalidOperationException(
                "Savings purpose can only be used when transferring to a Savings account.");
        }

        var balance = await _accountBalanceService
            .GetAccountBalanceAsync(
                dto.FromAccountId,
                userId);

        if (balance < dto.Amount)
        {
            throw new InvalidOperationException(
                "Insufficient balance.");
        }

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var transfer = new Transaction
            {
                UserId = userId,
                AccountId = null,
                FromAccountId = dto.FromAccountId,
                ToAccountId = dto.ToAccountId,
                Amount = dto.Amount,
                Type = TransactionType.Transfer,
                Purpose = dto.TransactionPurpose,
                TransactionDate = dto.TransactionDate,
                Description =
                    dto.Description ??
                    $"Transfer from {fromAccount.Name} to {toAccount.Name}",
                CreatedAt = DateTime.UtcNow
            };

            _context.Transactions.Add(transfer);

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }



    public async Task<List<TransactionListDto>> GetFilteredAsync(
        int userId,
        TransactionFilterDto filter)
    {
        var query = _context.Transactions
            .Include(t => t.Account)
            .Include(t => t.Category)
            .Where(t => t.UserId == userId)
            .AsQueryable();

        if (filter.FromDate.HasValue)
        {
            query = query.Where(t =>
                t.TransactionDate >= filter.FromDate.Value);
        }

        if (filter.ToDate.HasValue)
        {
            var toDate =
                filter.ToDate.Value.Date.AddDays(1);

            query = query.Where(t =>
                t.TransactionDate < toDate);
        }

        if (filter.AccountId.HasValue)
        {
            query = query.Where(t =>
                t.AccountId == filter.AccountId.Value);
        }

        if (filter.CategoryId.HasValue)
        {
            query = query.Where(t =>
                t.CategoryId == filter.CategoryId.Value);
        }

        if (filter.Type.HasValue)
        {
            query = query.Where(t =>
                t.Type == filter.Type.Value);
        }

        return await query
            .OrderByDescending(t => t.TransactionDate)
            .Select(t => new TransactionListDto
            {
                Id = t.Id,

                AccountId = t.AccountId,

                AccountName =
                    t.Account != null
                        ? t.Account.Name
                        : null,

                CategoryId = t.CategoryId,

                CategoryName =
                    t.Category != null
                        ? t.Category.Name
                        : null,

                Amount = t.Amount,

                Type = t.Type,

                Description = t.Description,

                TransactionDate = t.TransactionDate,

                Purpose = t.Purpose
            })
            .ToListAsync();
    }

    private static TransactionResponseDto MapToDto(
        Transaction transaction)
    {
        return new TransactionResponseDto
        {
            Id = transaction.Id,

            AccountId = transaction.AccountId,

            FromAccountId = transaction.FromAccountId,

            ToAccountId = transaction.ToAccountId,

            CategoryId = transaction.CategoryId,

            Amount = transaction.Amount,

            Type = transaction.Type,

            Description = transaction.Description,

            TransactionDate = transaction.TransactionDate,

            CreatedAt = transaction.CreatedAt
        };
    }
}