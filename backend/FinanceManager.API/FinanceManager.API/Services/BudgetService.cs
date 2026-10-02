using FinanceManager.API.DTOs;
using FinanceManager.API.Models;
using FinanceManager.API.Repositories.Interfaces;
using FinanceManager.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.API.Services;

public class BudgetService : IBudgetService
{
    private readonly IBudgetRepository _budgetRepository;
    private readonly ICategoryBudgetRepository _categoryBudgetRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ITransactionRepository _transactionRepository;

    public BudgetService(
        IBudgetRepository budgetRepository,
        ICategoryBudgetRepository categoryBudgetRepository,
        ICategoryRepository categoryRepository,
        ITransactionRepository transactionRepository)
    {
        _budgetRepository = budgetRepository;
        _categoryBudgetRepository = categoryBudgetRepository;
        _categoryRepository = categoryRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task CreateAsync(
        BudgetCreateDto dto,
        int userId)
    {
        if (dto.Month < 1 || dto.Month > 12)
        {
            throw new InvalidOperationException("Invalid month.");
        }

        if (dto.ExpectedIncome < 0 ||
            dto.ExpenseBudget < 0 ||
            dto.SavingsTarget < 0 ||
            dto.InvestmentTarget < 0)
        {
            throw new InvalidOperationException("Budget amounts cannot be negative.");
        }

        var existingBudget = await _budgetRepository.AnyAsync(b =>
            b.UserId == userId &&
            b.Year == dto.Year &&
            b.Month == dto.Month);

        if (existingBudget)
        {
            throw new InvalidOperationException("Budget already exists for this month.");
        }

        var categoryIds = dto.Categories
            .Select(c => c.CategoryId)
            .Distinct()
            .ToList();

        var validCategoryCount = await _categoryRepository.CountAsync(c =>
            c.UserId == userId &&
            categoryIds.Contains(c.Id));

        if (validCategoryCount != categoryIds.Count)
        {
            throw new InvalidOperationException("One or more categories are invalid.");
        }

        if (dto.Categories.Any(c => c.Amount < 0))
        {
            throw new InvalidOperationException("Category budget amounts cannot be negative.");
        }

        var categoryBudgetTotal = dto.Categories.Sum(c => c.Amount);

        if (categoryBudgetTotal > dto.ExpenseBudget)
        {
            throw new InvalidOperationException("Category budgets cannot exceed total expense budget.");
        }

        var budget = new Budget
        {
            UserId = userId,
            Year = dto.Year,
            Month = dto.Month,
            ExpectedIncome = dto.ExpectedIncome,
            ExpenseBudget = dto.ExpenseBudget,
            SavingsTarget = dto.SavingsTarget,
            InvestmentTarget = dto.InvestmentTarget
        };

        foreach (var category in dto.Categories)
        {
            budget.CategoryBudgets.Add(new CategoryBudget
            {
                CategoryId = category.CategoryId,
                Amount = category.Amount
            });
        }

        await _budgetRepository.AddAsync(budget);
        await _budgetRepository.SaveChangesAsync();
    }

    public async Task<BudgetResponseDto?> GetAsync(
        int year,
        int month,
        int userId)
    {
        var budget = await _budgetRepository.Query()
            .Include(b => b.CategoryBudgets)
                .ThenInclude(cb => cb.Category)
            .FirstOrDefaultAsync(b =>
                b.UserId == userId &&
                b.Year == year &&
                b.Month == month);

        if (budget == null)
        {
            return null;
        }

        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1);

        var spent = await _transactionRepository.Query()
            .Where(t =>
                t.UserId == userId &&
                (
                    t.Type == TransactionType.Expense ||
                    t.Type == TransactionType.CreditCard
                ) &&
                t.TransactionDate >= startDate &&
                t.TransactionDate < endDate &&
                t.Purpose != TransactionPurpose.LoanRepayment &&
                t.Purpose != TransactionPurpose.LoanLent)
            .SumAsync(t => (decimal?)t.Amount) ?? 0;

        var remaining = budget.ExpenseBudget - spent;

        return new BudgetResponseDto
        {
            Id = budget.Id,
            Year = budget.Year,
            Month = budget.Month,
            Income = budget.ExpectedIncome,
            ExpenseBudget = budget.ExpenseBudget,
            SavingsTarget = budget.SavingsTarget,
            InvestmentTarget = budget.InvestmentTarget,
            Spent = spent,
            Remaining = remaining,
            Categories = budget.CategoryBudgets
                .Select(cb => new BudgetCategoryResponseDto
                {
                    CategoryId = cb.CategoryId,
                    CategoryName = cb.Category.Name,
                    Amount = cb.Amount
                })
                .ToList()
        };
    }

    public async Task UpdateAsync(
        int year,
        int month,
        BudgetCreateDto dto,
        int userId)
    {
        if (month < 1 || month > 12)
        {
            throw new InvalidOperationException("Invalid month.");
        }

        if (dto.ExpectedIncome < 0 ||
            dto.ExpenseBudget < 0 ||
            dto.SavingsTarget < 0 ||
            dto.InvestmentTarget < 0)
        {
            throw new InvalidOperationException("Budget amounts cannot be negative.");
        }

        var budget = await _budgetRepository.Query()
            .Include(b => b.CategoryBudgets)
            .FirstOrDefaultAsync(b =>
                b.UserId == userId &&
                b.Year == year &&
                b.Month == month);

        if (budget == null)
        {
            throw new InvalidOperationException("Budget not found.");
        }

        var categoryIds = dto.Categories
            .Select(c => c.CategoryId)
            .Distinct()
            .ToList();

        var validCategoryCount = await _categoryRepository.CountAsync(c =>
            c.UserId == userId &&
            categoryIds.Contains(c.Id));

        if (validCategoryCount != categoryIds.Count)
        {
            throw new InvalidOperationException("One or more categories are invalid.");
        }

        if (dto.Categories.Any(c => c.Amount < 0))
        {
            throw new InvalidOperationException("Category budget amounts cannot be negative.");
        }

        var categoryBudgetTotal = dto.Categories.Sum(c => c.Amount);

        if (categoryBudgetTotal > dto.ExpenseBudget)
        {
            throw new InvalidOperationException("Category budgets cannot exceed total expense budget.");
        }

        budget.ExpectedIncome = dto.ExpectedIncome;
        budget.ExpenseBudget = dto.ExpenseBudget;
        budget.SavingsTarget = dto.SavingsTarget;
        budget.InvestmentTarget = dto.InvestmentTarget;

        _categoryBudgetRepository.RemoveRange(budget.CategoryBudgets);

        foreach (var category in dto.Categories)
        {
            budget.CategoryBudgets.Add(new CategoryBudget
            {
                BudgetId = budget.Id,
                CategoryId = category.CategoryId,
                Amount = category.Amount
            });
        }

        await _budgetRepository.SaveChangesAsync();
    }
}