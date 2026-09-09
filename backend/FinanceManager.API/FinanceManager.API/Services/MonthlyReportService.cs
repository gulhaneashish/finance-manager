using FinanceManager.API.Data;
using FinanceManager.API.DTOs;
using FinanceManager.API.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.API.Services;

public class MonthlyReportService
{
    private readonly FinanceDbContext _context;

    public MonthlyReportService(FinanceDbContext context)
    {
        _context = context;
    }

    public async Task<MonthlyReportDto> GetReportAsync(
        int userId,
        int year,
        int month)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1);

        var transactions = await _context.Transactions
            .Include(t => t.Category)
            .Where(t =>
                t.UserId == userId &&
                t.TransactionDate >= startDate &&
                t.TransactionDate < endDate)
            .ToListAsync();

        var income = transactions
            .Where(t =>
                t.Type == TransactionType.Income &&
                t.Purpose != TransactionPurpose.Deposit &&
                t.Purpose != TransactionPurpose.LoanBorrowed &&
                t.Purpose != TransactionPurpose.LoanReceived)
            .Sum(t => t.Amount);

        var expenses = transactions
     .Where(t =>
         (t.Type == TransactionType.Expense ||
          t.Type == TransactionType.CreditCard) &&
         t.Purpose != TransactionPurpose.Investment &&
         t.Purpose != TransactionPurpose.LoanLent &&
         t.Purpose != TransactionPurpose.LoanRepayment &&
         t.Purpose != TransactionPurpose.LoanPayment)
     .Sum(t => t.Amount);

        var budget = await _context.Budgets
            .Include(b => b.CategoryBudgets)
            .ThenInclude(cb => cb.Category)
            .FirstOrDefaultAsync(b =>
                b.UserId == userId &&
                b.Year == year &&
                b.Month == month);

        var expenseBudget =
            budget?.ExpenseBudget ?? 0;

        var savingsTarget =
            budget?.SavingsTarget ?? 0;

        var investmentTarget =
            budget?.InvestmentTarget ?? 0;

        var categoryReports =
            new List<CategoryReportDto>();

        if (budget != null)
        {
            foreach (var categoryBudget
                     in budget.CategoryBudgets)
            {
                var spent = transactions
     .Where(t =>
         (t.Type == TransactionType.Expense ||
          t.Type == TransactionType.CreditCard) &&
         t.CategoryId == categoryBudget.CategoryId &&
         t.Purpose != TransactionPurpose.Investment &&
         t.Purpose != TransactionPurpose.LoanLent &&
         t.Purpose != TransactionPurpose.LoanRepayment &&
         t.Purpose != TransactionPurpose.LoanPayment)
     .Sum(t => t.Amount);

                var remaining =
                    categoryBudget.Amount - spent;

                var percentage =
                    categoryBudget.Amount == 0
                        ? 0
                        : (spent /
                           categoryBudget.Amount) * 100;

                categoryReports.Add(
                    new CategoryReportDto
                    {
                        CategoryId =
                            categoryBudget.CategoryId,

                        CategoryName =
                            categoryBudget.Category.Name,

                        Budget =
                            categoryBudget.Amount,

                        Spent =
                            spent,

                        Remaining =
                            remaining,

                        PercentageUsed =
                            percentage
                    });
            }
        }

        var borrowed = await _context.Loans
            .Where(l =>
                l.UserId == userId &&
                l.Type == LoanType.Borrowed &&
                l.IsActive)
            .Include(l => l.Payments)
            .ToListAsync();

        var lent = await _context.Loans
            .Where(l =>
                l.UserId == userId &&
                l.Type == LoanType.Lent &&
                l.IsActive)
            .Include(l => l.Payments)
            .ToListAsync();

        var totalBorrowed =
            borrowed.Sum(l =>
                Math.Max(
                    l.OriginalAmount -
                    l.Payments.Sum(p => p.Amount),
                    0));

        var totalLent =
            lent.Sum(l =>
                Math.Max(
                    l.OriginalAmount -
                    l.Payments.Sum(p => p.Amount),
                    0));

        // Savings during selected month
        var savingsAmount =
            CalculateSavings(transactions);

        // Investment during selected month
        var investmentAmount =
            await CalculateInvestmentsAsync(
                userId,
                startDate,
                endDate);

        return new MonthlyReportDto
        {
            Year = year,
            Month = month,

            TotalIncome =
                income,

            TotalExpenses =
                expenses,

            NetCashFlow =
                income - expenses,

            ExpenseBudget =
                expenseBudget,

            BudgetSpent =
                expenses,

            BudgetRemaining =
                expenseBudget - expenses,

            SavingsTarget =
                savingsTarget,

            InvestmentTarget =
                investmentTarget,

            SavingsAmount =
                savingsAmount,

            InvestmentAmount =
                investmentAmount,

            TotalBorrowed =
                totalBorrowed,

            TotalLent =
                totalLent,

            Categories =
                categoryReports
        };
    }

    private decimal CalculateSavings(
        List<Transaction> transactions)
    {
        return transactions
            .Where(t =>
                t.Type == TransactionType.Transfer &&
                t.Purpose == TransactionPurpose.Savings)
            .Sum(t => t.Amount);
    }

    private async Task<decimal> CalculateInvestmentsAsync(
        int userId,
        DateTime startDate,
        DateTime endDate)
    {
        return await _context.Investments
            .Where(i =>
                i.UserId == userId &&
                i.IsActive &&
                i.InvestmentDate >= startDate &&
                i.InvestmentDate < endDate)
            .SumAsync(i =>
                (decimal?)i.InvestedAmount) ?? 0;
    }
}