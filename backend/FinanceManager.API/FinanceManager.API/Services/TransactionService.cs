using FinanceManager.API.Common;
using FinanceManager.API.DTOs;
using FinanceManager.API.Models;
using FinanceManager.API.Repositories.Interfaces;
using FinanceManager.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.API.Services;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IAccountBalanceService _accountBalanceService;
    private readonly INotificationService? _notificationService;
    private readonly IBudgetRepository? _budgetRepository;

    public TransactionService(
        ITransactionRepository transactionRepository,
        IAccountRepository accountRepository,
        ICategoryRepository categoryRepository,
        IAccountBalanceService accountBalanceService)
        : this(transactionRepository, accountRepository, categoryRepository, accountBalanceService, null, null)
    {
    }

    public TransactionService(
        ITransactionRepository transactionRepository,
        IAccountRepository accountRepository,
        ICategoryRepository categoryRepository,
        IAccountBalanceService accountBalanceService,
        INotificationService? notificationService = null,
        IBudgetRepository? budgetRepository = null)
    {
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
        _categoryRepository = categoryRepository;
        _accountBalanceService = accountBalanceService;
        _notificationService = notificationService;
        _budgetRepository = budgetRepository;
    }

    public async Task<TransactionResponseDto> CreateAsync(
        TransactionCreateDto dto,
        int userId)
    {
        if (dto.Amount <= 0)
        {
            throw new InvalidOperationException("Transaction amount must be greater than zero.");
        }

        if (dto.Type != TransactionType.Income &&
            dto.Type != TransactionType.Expense &&
            dto.Type != TransactionType.Transfer)
        {
            throw new InvalidOperationException("Invalid transaction type.");
        }

        if (dto.Type == TransactionType.Transfer)
        {
            if (!dto.FromAccountId.HasValue ||
                !dto.ToAccountId.HasValue)
            {
                throw new InvalidOperationException("Transfer requires source and destination accounts.");
            }

            if (dto.FromAccountId == dto.ToAccountId)
            {
                throw new InvalidOperationException("Source and destination accounts must be different.");
            }

            var accounts = await _accountRepository.Query()
                .Where(a =>
                    (a.Id == dto.FromAccountId.Value ||
                     a.Id == dto.ToAccountId.Value) &&
                    a.UserId == userId &&
                    a.IsActive)
                .ToListAsync();

            if (accounts.Count != 2)
            {
                throw new InvalidOperationException("Invalid source or destination account.");
            }

            var fromAccount = accounts
                .First(a => a.Id == dto.FromAccountId.Value);

            var toAccount = accounts
                .First(a => a.Id == dto.ToAccountId.Value);

            if (fromAccount.AccountType == "CREDIT_CARD")
            {
                throw new InvalidOperationException("Credit card cannot be used as a transfer source.");
            }

            if (toAccount.AccountType == "CREDIT_CARD")
            {
                throw new InvalidOperationException("You cannot transfer money directly to a credit card. Use Credit Card Payment instead.");
            }

            var balance = await _accountBalanceService.GetAccountBalanceAsync(
                dto.FromAccountId.Value,
                userId);

            if (balance < dto.Amount)
            {
                throw new InvalidOperationException("Insufficient balance.");
            }
        }
        else
        {
            if (!dto.AccountId.HasValue)
            {
                throw new InvalidOperationException("Account is required.");
            }

            var account = await _accountRepository.FirstOrDefaultAsync(a =>
                a.Id == dto.AccountId.Value &&
                a.UserId == userId &&
                a.IsActive);

            if (account == null)
            {
                throw new InvalidOperationException("Account not found or inactive.");
            }

            if (account.AccountType == "CREDIT_CARD")
            {
                throw new InvalidOperationException("Use Credit Card Purchase for credit card transactions.");
            }
        }

        if (dto.CategoryId.HasValue)
        {
            var categoryExists = await _categoryRepository.AnyAsync(c =>
                c.Id == dto.CategoryId.Value &&
                c.UserId == userId);

            if (!categoryExists)
            {
                throw new InvalidOperationException("Category does not belong to the current user.");
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
            TransactionDate = dto.TransactionDate.ToUniversalUtc(),
            CreatedAt = DateTime.UtcNow
        };

        await _transactionRepository.AddAsync(transaction);
        await _transactionRepository.SaveChangesAsync();

        if (_notificationService != null)
        {
            var typeText = transaction.Type.ToString();
            var title = transaction.Type == TransactionType.Income ? "💰 Income Recorded" : "💸 Expense Recorded";
            var message = $"₹{transaction.Amount:N2} {typeText.ToLower()} recorded successfully.";
            await _notificationService.SendTransactionAlertAsync(userId, title, message, new { transactionId = transaction.Id, amount = transaction.Amount, type = typeText });

            if (transaction.Type == TransactionType.Expense && transaction.CategoryId.HasValue && _budgetRepository != null)
            {
                await CheckBudgetAlertAsync(userId, transaction.CategoryId.Value, transaction.TransactionDate);
            }
        }

        return MapToDto(transaction);
    }

    public async Task<List<TransactionResponseDto>> GetAllAsync(
        int userId)
    {
        var transactions = await _transactionRepository.Query()
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.Id)
            .ToListAsync();

        return transactions
            .Select(MapToDto)
            .ToList();
    }

    public async Task<TransactionResponseDto?> GetByIdAsync(
        int id,
        int userId)
    {
        var transaction = await _transactionRepository.FirstOrDefaultAsync(t =>
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
        var transaction = await _transactionRepository.FirstOrDefaultAsync(t =>
            t.Id == id &&
            t.UserId == userId);

        if (transaction == null)
        {
            return false;
        }

        _transactionRepository.Remove(transaction);
        await _transactionRepository.SaveChangesAsync();

        return true;
    }

    public async Task CreateTransferAsync(
        TransferCreateDto dto,
        int userId)
    {
        if (dto.FromAccountId == dto.ToAccountId)
        {
            throw new InvalidOperationException("Source and destination accounts must be different.");
        }

        if (dto.Amount <= 0)
        {
            throw new InvalidOperationException("Transfer amount must be greater than zero.");
        }

        var fromAccount = await _accountRepository.FirstOrDefaultAsync(a =>
            a.Id == dto.FromAccountId &&
            a.UserId == userId &&
            a.IsActive);

        var toAccount = await _accountRepository.FirstOrDefaultAsync(a =>
            a.Id == dto.ToAccountId &&
            a.UserId == userId &&
            a.IsActive);

        if (fromAccount == null || toAccount == null)
        {
            throw new InvalidOperationException("Invalid account.");
        }

        if (fromAccount.AccountType == "CREDIT_CARD")
        {
            throw new InvalidOperationException("Credit card cannot be used as a transfer source.");
        }

        if (toAccount.AccountType == "CREDIT_CARD")
        {
            throw new InvalidOperationException("You cannot transfer money directly to a credit card. Use Credit Card Payment instead.");
        }

        if (dto.TransactionPurpose != TransactionPurpose.Transfer &&
            dto.TransactionPurpose != TransactionPurpose.Savings)
        {
            throw new InvalidOperationException("Invalid transfer purpose.");
        }

        if (dto.TransactionPurpose == TransactionPurpose.Savings &&
            toAccount.AccountType != "SAVINGS")
        {
            throw new InvalidOperationException("Savings purpose can only be used when transferring to a Savings account.");
        }

        var balance = await _accountBalanceService.GetAccountBalanceAsync(
            dto.FromAccountId,
            userId);

        if (balance < dto.Amount)
        {
            throw new InvalidOperationException("Insufficient balance.");
        }

        await using var transaction =
            await _transactionRepository.BeginTransactionAsync();

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
                TransactionDate = dto.TransactionDate.ToUniversalUtc(),
                Description = dto.Description ?? $"Transfer from {fromAccount.Name} to {toAccount.Name}",
                CreatedAt = DateTime.UtcNow
            };

            await _transactionRepository.AddAsync(transfer);
            await _transactionRepository.SaveChangesAsync();

            await transaction.CommitAsync();

            if (_notificationService != null)
            {
                await _notificationService.SendTransactionAlertAsync(userId, "↔️ Transfer Completed",
                    $"Transferred ₹{dto.Amount:N2} from {fromAccount.Name} to {toAccount.Name}.",
                    new { fromAccountId = dto.FromAccountId, toAccountId = dto.ToAccountId, amount = dto.Amount });
            }
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
        var query = _transactionRepository.Query()
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
            var toDate = filter.ToDate.Value.Date.AddDays(1);
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
            .ThenByDescending(t => t.Id)
            .Select(t => new TransactionListDto
            {
                Id = t.Id,
                AccountId = t.AccountId,
                AccountName = t.Account != null ? t.Account.Name : null,
                CategoryId = t.CategoryId,
                CategoryName = t.Category != null ? t.Category.Name : null,
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

    private async Task CheckBudgetAlertAsync(int userId, int categoryId, DateTime date)
    {
        try
        {
            var year = date.Year;
            var month = date.Month;

            var budget = await _budgetRepository!.Query()
                .Include(b => b.CategoryBudgets)
                .FirstOrDefaultAsync(b => b.UserId == userId && b.Year == year && b.Month == month);

            if (budget == null) return;

            var categoryBudget = budget.CategoryBudgets.FirstOrDefault(cb => cb.CategoryId == categoryId);
            if (categoryBudget == null || categoryBudget.Amount <= 0) return;

            var category = await _categoryRepository.FirstOrDefaultAsync(c => c.Id == categoryId && c.UserId == userId);
            var categoryName = category?.Name ?? "Category";

            var startDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
            var endDate = startDate.AddMonths(1);

            var totalSpent = await _transactionRepository.Query()
                .Where(t => t.UserId == userId &&
                            t.CategoryId == categoryId &&
                            t.Type == TransactionType.Expense &&
                            t.TransactionDate >= startDate &&
                            t.TransactionDate < endDate)
                .SumAsync(t => (decimal?)t.Amount) ?? 0;

            var percentage = (totalSpent / categoryBudget.Amount) * 100m;

            if (percentage >= 80m && _notificationService != null)
            {
                await _notificationService.SendBudgetAlertAsync(userId, categoryName, totalSpent, categoryBudget.Amount, percentage);
            }
        }
        catch
        {
            // Non-critical background alert failure should not affect transaction flow
        }
    }
}