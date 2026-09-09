//using FinanceManager.API.Data;
//using FinanceManager.API.DTOs;
//using FinanceManager.API.Models;
//using Microsoft.EntityFrameworkCore;

//namespace FinanceManager.API.Services;

//public class BudgetService
//{
//    private readonly FinanceDbContext _context;

//    public BudgetService(FinanceDbContext context)
//    {
//        _context = context;
//    }

//    public async Task CreateAsync(
//        BudgetCreateDto dto,
//        int userId)
//    {
//        if (dto.Year < 2000 || dto.Year > 2100)
//        {
//            throw new InvalidOperationException(
//                "Invalid year.");
//        }

//        if (dto.Month < 1 || dto.Month > 12)
//        {
//            throw new InvalidOperationException(
//                "Invalid month.");
//        }

//        if (dto.ExpectedIncome < 0 ||
//            dto.ExpenseBudget < 0 ||
//            dto.SavingsTarget < 0 ||
//            dto.InvestmentTarget < 0)
//        {
//            throw new InvalidOperationException(
//                "Budget amounts cannot be negative.");
//        }

//        var existingBudget =
//            await _context.Budgets
//                .AnyAsync(b =>
//                    b.UserId == userId &&
//                    b.Year == dto.Year &&
//                    b.Month == dto.Month);

//        if (existingBudget)
//        {
//            throw new InvalidOperationException(
//                "Budget already exists for this month.");
//        }

//        var categoryIds = dto.Categories
//            .Select(c => c.CategoryId)
//            .Distinct()
//            .ToList();

//        var validCategoryCount =
//            await _context.Categories
//                .CountAsync(c =>
//                    c.UserId == userId &&
//                    categoryIds.Contains(c.Id));

//        if (validCategoryCount != categoryIds.Count)
//        {
//            throw new InvalidOperationException(
//                "One or more categories are invalid.");
//        }

//        var categoryBudgetTotal =
//            dto.Categories.Sum(c => c.Amount);

//        if (categoryBudgetTotal > dto.ExpenseBudget)
//        {
//            throw new InvalidOperationException(
//                "Category budgets cannot exceed total expense budget.");
//        }

//        if (dto.Categories.Any(c => c.Amount < 0))
//        {
//            throw new InvalidOperationException(
//                "Category budget amounts cannot be negative.");
//        }

//        var budget = new Budget
//        {
//            UserId = userId,
//            Year = dto.Year,
//            Month = dto.Month,
//            ExpectedIncome = dto.ExpectedIncome,
//            ExpenseBudget = dto.ExpenseBudget,
//            SavingsTarget = dto.SavingsTarget,
//            InvestmentTarget = dto.InvestmentTarget
//        };

//        foreach (var category in dto.Categories)
//        {
//            budget.CategoryBudgets.Add(
//                new CategoryBudget
//                {
//                    CategoryId = category.CategoryId,
//                    Amount = category.Amount
//                });
//        }

//        _context.Budgets.Add(budget);

//        await _context.SaveChangesAsync();
//    }

//    public async Task<BudgetResponseDto?> GetAsync(
//        int year,
//        int month,
//        int userId)
//    {
//        var budget = await _context.Budgets
//            .Include(b => b.CategoryBudgets)
//                .ThenInclude(cb => cb.Category)
//            .FirstOrDefaultAsync(b =>
//                b.UserId == userId &&
//                b.Year == year &&
//                b.Month == month);

//        if (budget == null)
//        {
//            return null;
//        }

//        var startDate = new DateTime(year, month, 1);
//        var endDate = startDate.AddMonths(1);

//        var spent = await _context.Transactions
//            .Where(t =>
//                t.UserId == userId &&
//                (
//                    t.Type == TransactionType.Expense ||
//                    t.Type == TransactionType.CreditCard
//                ) &&
//                t.TransactionDate >= startDate &&
//                t.TransactionDate < endDate &&
//                t.Purpose != TransactionPurpose.LoanRepayment &&
//                t.Purpose != TransactionPurpose.LoanLent)
//            .SumAsync(t => (decimal?)t.Amount) ?? 0;

//        var remaining =
//            budget.ExpenseBudget - spent;

//        return new BudgetResponseDto
//        {
//            Id = budget.Id,

//            Year = budget.Year,

//            Month = budget.Month,

//            Income = budget.ExpectedIncome,

//            ExpenseBudget = budget.ExpenseBudget,

//            SavingsTarget = budget.SavingsTarget,

//            InvestmentTarget = budget.InvestmentTarget,

//            Spent = spent,

//            Remaining = remaining,

//            Categories = budget.CategoryBudgets
//                .Select(cb => new BudgetCategoryResponseDto
//                {
//                    CategoryId = cb.CategoryId,

//                    CategoryName = cb.Category.Name,

//                    Amount = cb.Amount
//                })
//                .ToList()
//        };
//    }

//    public async Task UpdateAsync(
//        int year,
//        int month,
//        BudgetCreateDto dto,
//        int userId)
//    {
//        if (month < 1 || month > 12)
//        {
//            throw new InvalidOperationException(
//                "Invalid month.");
//        }

//        var budget = await _context.Budgets
//            .Include(b => b.CategoryBudgets)
//            .FirstOrDefaultAsync(b =>
//                b.UserId == userId &&
//                b.Year == year &&
//                b.Month == month);

//        if (budget == null)
//        {
//            throw new InvalidOperationException(
//                "Budget not found.");
//        }

//        if (dto.ExpectedIncome < 0 ||
//            dto.ExpenseBudget < 0 ||
//            dto.SavingsTarget < 0 ||
//            dto.InvestmentTarget < 0)
//        {
//            throw new InvalidOperationException(
//                "Budget amounts cannot be negative.");
//        }

//        var categoryIds = dto.Categories
//            .Select(c => c.CategoryId)
//            .Distinct()
//            .ToList();

//        var validCategoryCount =
//            await _context.Categories
//                .CountAsync(c =>
//                    c.UserId == userId &&
//                    categoryIds.Contains(c.Id));

//        if (validCategoryCount != categoryIds.Count)
//        {
//            throw new InvalidOperationException(
//                "One or more categories are invalid.");
//        }

//        if (dto.Categories.Any(c => c.Amount < 0))
//        {
//            throw new InvalidOperationException(
//                "Category budget amounts cannot be negative.");
//        }

//        var categoryBudgetTotal =
//            dto.Categories.Sum(c => c.Amount);

//        if (categoryBudgetTotal > dto.ExpenseBudget)
//        {
//            throw new InvalidOperationException(
//                "Category budgets cannot exceed total expense budget.");
//        }

//        budget.ExpectedIncome =
//            dto.ExpectedIncome;

//        budget.ExpenseBudget =
//            dto.ExpenseBudget;

//        budget.SavingsTarget =
//            dto.SavingsTarget;

//        budget.InvestmentTarget =
//            dto.InvestmentTarget;

//        _context.CategoryBudgets.RemoveRange(
//            budget.CategoryBudgets);

//        foreach (var category in dto.Categories)
//        {
//            budget.CategoryBudgets.Add(
//                new CategoryBudget
//                {
//                    BudgetId = budget.Id,
//                    CategoryId = category.CategoryId,
//                    Amount = category.Amount
//                });
//        }

//        await _context.SaveChangesAsync();
//    }
//}

using FinanceManager.API.Data;
using FinanceManager.API.DTOs;
using FinanceManager.API.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.API.Services;

public class BudgetService
{
    private readonly FinanceDbContext _context;

    public BudgetService(FinanceDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(
        BudgetCreateDto dto,
        int userId)
    {
        if (dto.Month < 1 || dto.Month > 12)
        {
            throw new InvalidOperationException(
                "Invalid month.");
        }

        if (dto.ExpectedIncome < 0 ||
            dto.ExpenseBudget < 0 ||
            dto.SavingsTarget < 0 ||
            dto.InvestmentTarget < 0)
        {
            throw new InvalidOperationException(
                "Budget amounts cannot be negative.");
        }

        var existingBudget =
            await _context.Budgets
                .AnyAsync(b =>
                    b.UserId == userId &&
                    b.Year == dto.Year &&
                    b.Month == dto.Month);

        if (existingBudget)
        {
            throw new InvalidOperationException(
                "Budget already exists for this month.");
        }

        var categoryIds = dto.Categories
            .Select(c => c.CategoryId)
            .Distinct()
            .ToList();

        var validCategoryCount =
            await _context.Categories
                .CountAsync(c =>
                    c.UserId == userId &&
                    categoryIds.Contains(c.Id));

        if (validCategoryCount != categoryIds.Count)
        {
            throw new InvalidOperationException(
                "One or more categories are invalid.");
        }

        if (dto.Categories.Any(c => c.Amount < 0))
        {
            throw new InvalidOperationException(
                "Category budget amounts cannot be negative.");
        }

        var categoryBudgetTotal =
            dto.Categories.Sum(c => c.Amount);

        if (categoryBudgetTotal > dto.ExpenseBudget)
        {
            throw new InvalidOperationException(
                "Category budgets cannot exceed total expense budget.");
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
            budget.CategoryBudgets.Add(
                new CategoryBudget
                {
                    CategoryId = category.CategoryId,

                    Amount = category.Amount
                });
        }

        _context.Budgets.Add(budget);

        await _context.SaveChangesAsync();
    }

    public async Task<BudgetResponseDto?> GetAsync(
        int year,
        int month,
        int userId)
    {
        var budget = await _context.Budgets
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

        var startDate =
            new DateTime(year, month, 1);

        var endDate =
            startDate.AddMonths(1);

        var spent = await _context.Transactions
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

        var remaining =
            budget.ExpenseBudget - spent;

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
            throw new InvalidOperationException(
                "Invalid month.");
        }

        if (dto.ExpectedIncome < 0 ||
            dto.ExpenseBudget < 0 ||
            dto.SavingsTarget < 0 ||
            dto.InvestmentTarget < 0)
        {
            throw new InvalidOperationException(
                "Budget amounts cannot be negative.");
        }

        var budget = await _context.Budgets
            .Include(b => b.CategoryBudgets)
            .FirstOrDefaultAsync(b =>
                b.UserId == userId &&
                b.Year == year &&
                b.Month == month);

        if (budget == null)
        {
            throw new InvalidOperationException(
                "Budget not found.");
        }

        var categoryIds = dto.Categories
            .Select(c => c.CategoryId)
            .Distinct()
            .ToList();

        var validCategoryCount =
            await _context.Categories
                .CountAsync(c =>
                    c.UserId == userId &&
                    categoryIds.Contains(c.Id));

        if (validCategoryCount != categoryIds.Count)
        {
            throw new InvalidOperationException(
                "One or more categories are invalid.");
        }

        if (dto.Categories.Any(c => c.Amount < 0))
        {
            throw new InvalidOperationException(
                "Category budget amounts cannot be negative.");
        }

        var categoryBudgetTotal =
            dto.Categories.Sum(c => c.Amount);

        if (categoryBudgetTotal > dto.ExpenseBudget)
        {
            throw new InvalidOperationException(
                "Category budgets cannot exceed total expense budget.");
        }

        budget.ExpectedIncome =
            dto.ExpectedIncome;

        budget.ExpenseBudget =
            dto.ExpenseBudget;

        budget.SavingsTarget =
            dto.SavingsTarget;

        budget.InvestmentTarget =
            dto.InvestmentTarget;

        _context.CategoryBudgets.RemoveRange(
            budget.CategoryBudgets);

        foreach (var category in dto.Categories)
        {
            budget.CategoryBudgets.Add(
                new CategoryBudget
                {
                    BudgetId = budget.Id,

                    CategoryId = category.CategoryId,

                    Amount = category.Amount
                });
        }

        await _context.SaveChangesAsync();
    }
}