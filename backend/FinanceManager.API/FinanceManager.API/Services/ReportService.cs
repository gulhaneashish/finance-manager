using FinanceManager.API.DTOs;
using FinanceManager.API.Models;
using FinanceManager.API.Repositories.Interfaces;
using FinanceManager.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.API.Services;

public class ReportService : IReportService
{
    private readonly ITransactionRepository _transactionRepository;

    public ReportService(ITransactionRepository transactionRepository)
    {
        _transactionRepository = transactionRepository;
    }

    public async Task<ReportSummaryDto> GetSummaryAsync(
        int userId,
        DateTime? fromDate,
        DateTime? toDate)
    {
        var query = _transactionRepository.Query()
            .Include(t => t.Category)
            .Where(t => t.UserId == userId)
            .AsQueryable();

        if (fromDate.HasValue)
        {
            query = query.Where(t =>
                t.TransactionDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            var endDate = toDate.Value.Date.AddDays(1);

            query = query.Where(t =>
                t.TransactionDate < endDate);
        }

        var transactions = await query.ToListAsync();

        var totalIncome = transactions
            .Where(t =>
                t.Type == TransactionType.Income &&
                t.Purpose != TransactionPurpose.Deposit &&
                t.Purpose != TransactionPurpose.LoanBorrowed &&
                t.Purpose != TransactionPurpose.LoanReceived &&
                t.Purpose != TransactionPurpose.InvestmentSale &&
                t.Purpose != TransactionPurpose.LoanRepayment)
            .Sum(t => t.Amount);

        var totalExpense = transactions
            .Where(t =>
                (t.Type == TransactionType.Expense ||
                 t.Type == TransactionType.CreditCard) &&
                t.Purpose != TransactionPurpose.Investment &&
                t.Purpose != TransactionPurpose.LoanLent &&
                t.Purpose != TransactionPurpose.LoanRepayment &&
                t.Purpose != TransactionPurpose.LoanPayment)
            .Sum(t => t.Amount);

        var categoryExpenses = transactions
            .Where(t =>
                (t.Type == TransactionType.Expense ||
                 t.Type == TransactionType.CreditCard) &&
                t.Purpose != TransactionPurpose.Investment &&
                t.Purpose != TransactionPurpose.LoanLent &&
                t.Purpose != TransactionPurpose.LoanRepayment &&
                t.Purpose != TransactionPurpose.LoanPayment)
            .GroupBy(t => new
            {
                t.CategoryId,
                CategoryName =
                    t.Category != null
                        ? t.Category.Name
                        : "Uncategorized"
            })
            .Select(g => new CategoryReportsDto
            {
                CategoryId = g.Key.CategoryId,
                CategoryName = g.Key.CategoryName,
                Amount = g.Sum(t => t.Amount)
            })
            .OrderByDescending(x => x.Amount)
            .ToList();

        return new ReportSummaryDto
        {
            TotalIncome = totalIncome,
            TotalExpense = totalExpense,
            NetAmount = totalIncome - totalExpense,
            CategoryExpenses = categoryExpenses
        };
    }
}