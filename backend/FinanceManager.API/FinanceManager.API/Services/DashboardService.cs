using FinanceManager.API.Data;
using FinanceManager.API.DTOs;
using FinanceManager.API.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.API.Services;

public class DashboardService
{
    private readonly FinanceDbContext _context;

    public DashboardService(FinanceDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(
        int userId,
        int year,
        int month)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1);

        var transactions = await _context.Transactions
            .Where(t => t.UserId == userId)
            .ToListAsync();

        var monthlyTransactions = transactions
    .Where(t =>
        t.TransactionDate >= startDate &&
        t.TransactionDate < endDate)
    .ToList();

        var income = monthlyTransactions
    .Where(t =>
        t.Type == TransactionType.Income &&
        t.Purpose != TransactionPurpose.Deposit &&
        t.Purpose != TransactionPurpose.LoanBorrowed &&
        t.Purpose != TransactionPurpose.LoanReceived &&
        t.Purpose != TransactionPurpose.InvestmentSale &&
        t.Purpose != TransactionPurpose.LoanRepayment)
    .Sum(t => t.Amount);

        var expenses = monthlyTransactions
     .Where(t =>
         (
             t.Type == TransactionType.Expense ||
             t.Type == TransactionType.CreditCard
         ) &&
         t.Purpose != TransactionPurpose.Investment &&
         t.Purpose != TransactionPurpose.LoanLent &&
         t.Purpose != TransactionPurpose.LoanRepayment)
     .Sum(t => t.Amount);

        var accounts = await _context.Accounts
            .Where(a => a.UserId == userId)
            .ToListAsync();

        decimal totalBalance = 0;

        foreach (var account in accounts)
        {
            if (account.AccountType == "CREDIT_CARD")
            {
                var purchases = transactions
                    .Where(t =>
                        t.AccountId == account.Id &&
                        t.Purpose == TransactionPurpose.CreditCardPurchase)
                    .Sum(t => t.Amount);

                var payments = transactions
                    .Where(t =>
                        t.ToAccountId == account.Id &&
                        t.Purpose == TransactionPurpose.CreditCardPayment)
                    .Sum(t => t.Amount);

                var outstanding = Math.Max(
                    purchases - payments,
                    0);

                totalBalance -= outstanding;

                continue;
            }

            var accountIncome = transactions
                .Where(t =>
                    t.AccountId == account.Id &&
                    t.Type == TransactionType.Income &&
                    t.Purpose != TransactionPurpose.Deposit)
                .Sum(t => t.Amount);

            var deposits = transactions
                .Where(t =>
                    t.AccountId == account.Id &&
                    t.Purpose == TransactionPurpose.Deposit)
                .Sum(t => t.Amount);

            var accountExpenses = transactions
                .Where(t =>
                    t.AccountId == account.Id &&
                    t.Type == TransactionType.Expense)
                .Sum(t => t.Amount);

            var transfersIn = transactions
                .Where(t =>
                    t.ToAccountId == account.Id &&
                    t.Type == TransactionType.Transfer)
                .Sum(t => t.Amount);

            var transfersOut = transactions
                .Where(t =>
                    t.FromAccountId == account.Id &&
                    t.Type == TransactionType.Transfer)
                .Sum(t => t.Amount);

            var creditCardPayments = transactions
                .Where(t =>
                    t.FromAccountId == account.Id &&
                    t.Purpose == TransactionPurpose.CreditCardPayment)
                .Sum(t => t.Amount);

            var balance =
                account.OpeningBalance
                + accountIncome
                + deposits
                - accountExpenses
                + transfersIn
                - transfersOut
                - creditCardPayments;

            totalBalance += balance;
        }

        var savings = monthlyTransactions
    .Where(t =>
        t.Purpose == TransactionPurpose.Savings)
    .Sum(t => t.Amount);
        var investments = await _context.Investments
    .Where(i =>
        i.UserId == userId &&
        i.IsActive &&
        i.InvestmentDate >= startDate &&
        i.InvestmentDate < endDate)
    .SumAsync(i => (decimal?)i.InvestedAmount) ?? 0;

        var budget = await _context.Budgets
            .FirstOrDefaultAsync(b =>
                b.UserId == userId &&
                b.Year == year &&
                b.Month == month);

        var budgetAmount = budget?.ExpenseBudget ?? 0;

        return new DashboardSummaryDto
        {
            Year = year,
            Month = month,

            TotalBalance = totalBalance,

            TotalIncome = income,

            TotalExpenses = expenses,

            ExpenseBudget = budgetAmount,

            BudgetSpent = expenses,

            BudgetRemaining = budgetAmount - expenses,

            SavingsTarget = budget?.SavingsTarget ?? 0,
            ActualSavings = savings,

            InvestmentTarget = budget?.InvestmentTarget ?? 0,
            ActualInvestment = investments
        };
    }

    public async Task<List<CategorySpendingDto>>
        GetCategorySpendingAsync(
            int userId,
            int year,
            int month)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1);

        var expenses = await _context.Transactions
     .Include(t => t.Category)
     .Where(t =>
         t.UserId == userId &&
         (
             t.Type == TransactionType.Expense ||
             t.Type == TransactionType.CreditCard
         ) &&
         t.TransactionDate >= startDate &&
         t.TransactionDate < endDate &&
         t.Purpose != TransactionPurpose.Investment &&
         t.Purpose != TransactionPurpose.LoanRepayment &&
         t.Purpose != TransactionPurpose.LoanLent)
     .ToListAsync();

        var totalExpenses = expenses.Sum(t => t.Amount);

        return expenses
            .GroupBy(t => new
            {
                t.CategoryId,
                CategoryName =
                    t.Category != null
                        ? t.Category.Name
                        : "Uncategorized"
            })
            .Select(g => new CategorySpendingDto
            {
                CategoryId = g.Key.CategoryId ?? 0,

                CategoryName = g.Key.CategoryName,

                Amount = g.Sum(t => t.Amount),

                Percentage =
                    totalExpenses == 0
                        ? 0
                        : Math.Round(
                            g.Sum(t => t.Amount) /
                            totalExpenses *
                            100,
                            2)
            })
            .OrderByDescending(x => x.Amount)
            .ToList();
    }

    public async Task<List<MonthlyCashFlowDto>>
        GetMonthlyCashFlowAsync(
            int userId,
            int year)
    {
        var startDate = new DateTime(year, 1, 1);
        var endDate = startDate.AddYears(1);

        var transactions = await _context.Transactions
            .Where(t =>
                t.UserId == userId &&
                t.TransactionDate >= startDate &&
                t.TransactionDate < endDate)
            .ToListAsync();

        return Enumerable
            .Range(1, 12)
            .Select(month =>
            {
                var monthlyTransactions = transactions
                    .Where(t => t.TransactionDate.Month == month)
                    .ToList();

                var income = monthlyTransactions
    .Where(t =>
        t.Type == TransactionType.Income &&
        t.Purpose != TransactionPurpose.Deposit &&
        t.Purpose != TransactionPurpose.LoanBorrowed &&
        t.Purpose != TransactionPurpose.LoanReceived &&
        t.Purpose != TransactionPurpose.InvestmentSale &&
        t.Purpose != TransactionPurpose.LoanRepayment)
    .Sum(t => t.Amount);

                var expenses = monthlyTransactions
     .Where(t =>
         (
             t.Type == TransactionType.Expense ||
             t.Type == TransactionType.CreditCard
         ) &&
         t.Purpose != TransactionPurpose.Investment &&
         t.Purpose != TransactionPurpose.LoanRepayment &&
         t.Purpose != TransactionPurpose.LoanLent)
     .Sum(t => t.Amount);

                return new MonthlyCashFlowDto
                {
                    Year = year,

                    Month = month,

                    MonthName = new DateTime(
                        year,
                        month,
                        1)
                        .ToString("MMM"),

                    Income = income,

                    Expenses = expenses,

                    Savings = income - expenses
                };
            })
            .ToList();
    }

    public async Task<LoanDebtSummaryDto>
        GetLoanDebtSummaryAsync(int userId)
    {
        var loans = await _context.Loans
            .Include(l => l.Payments)
            .Where(l =>
                l.UserId == userId &&
                l.IsActive)
            .OrderBy(l => l.DueDate)
            .ToListAsync();

        var today = DateTime.Today;

        var loanItems = loans
            .Select(loan =>
            {
                var paidAmount =
                    loan.Payments.Sum(p => p.Amount);

                var remainingAmount =
                    Math.Max(
                        loan.OriginalAmount -
                        paidAmount,
                        0);

                var isOverdue =
                    loan.DueDate.HasValue &&
                    loan.DueDate.Value.Date < today &&
                    remainingAmount > 0;

                return new LoanDebtItemDto
                {
                    LoanId = loan.Id,

                    PersonName = loan.PersonName,

                    Type = loan.Type,

                    OriginalAmount =
                        loan.OriginalAmount,

                    PaidAmount =
                        paidAmount,

                    RemainingAmount =
                        remainingAmount,

                    LoanDate = loan.LoanDate,

                    DueDate = loan.DueDate,

                    IsOverdue = isOverdue,

                    Notes = loan.Notes
                };
            })
            .ToList();

        var totalBorrowed = loanItems
            .Where(l => l.Type == LoanType.Borrowed)
            .Sum(l => l.OriginalAmount);

        var totalLent = loanItems
            .Where(l => l.Type == LoanType.Lent)
            .Sum(l => l.OriginalAmount);

        var totalOwedByMe = loanItems
            .Where(l => l.Type == LoanType.Borrowed)
            .Sum(l => l.RemainingAmount);

        var totalOwedToMe = loanItems
            .Where(l => l.Type == LoanType.Lent)
            .Sum(l => l.RemainingAmount);

        return new LoanDebtSummaryDto
        {
            TotalBorrowed = totalBorrowed,

            TotalLent = totalLent,

            TotalOwedByMe = totalOwedByMe,

            TotalOwedToMe = totalOwedToMe,

            NetDebt =
                totalOwedByMe -
                totalOwedToMe,

            Loans = loanItems
        };
    }

    public async Task<List<AccountSummaryDto>>
        GetAccountSummaryAsync(int userId)
    {
        var accounts = await _context.Accounts
            .Where(a =>
                a.UserId == userId &&
                a.IsActive)
            .ToListAsync();

        var transactions = await _context.Transactions
            .Where(t => t.UserId == userId)
            .ToListAsync();

        var result = new List<AccountSummaryDto>();

        foreach (var account in accounts)
        {
            decimal balance;
            decimal outstanding = 0;
            decimal availableCredit = 0;

            if (account.AccountType == "CREDIT_CARD")
            {
                var purchases = transactions
                    .Where(t =>
                        t.AccountId == account.Id &&
                        t.Purpose ==
                            TransactionPurpose.CreditCardPurchase)
                    .Sum(t => t.Amount);

                var payments = transactions
                    .Where(t =>
                        t.ToAccountId == account.Id &&
                        t.Purpose ==
                            TransactionPurpose.CreditCardPayment)
                    .Sum(t => t.Amount);

                outstanding = Math.Max(
                    purchases - payments,
                    0);

                balance = -outstanding;

                if (account.CreditLimit.HasValue)
                {
                    availableCredit = Math.Max(
                        account.CreditLimit.Value -
                        outstanding,
                        0);
                }
            }
            else
            {
                var accountIncome = transactions
                    .Where(t =>
                        t.AccountId == account.Id &&
                        t.Type == TransactionType.Income &&
                        t.Purpose !=
                            TransactionPurpose.Deposit)
                    .Sum(t => t.Amount);

                var deposits = transactions
                    .Where(t =>
                        t.AccountId == account.Id &&
                        t.Purpose ==
                            TransactionPurpose.Deposit)
                    .Sum(t => t.Amount);

                var accountExpenses = transactions
                    .Where(t =>
                        t.AccountId == account.Id &&
                        t.Type == TransactionType.Expense)
                    .Sum(t => t.Amount);

                var transfersIn = transactions
                    .Where(t =>
                        t.ToAccountId == account.Id &&
                        t.Type == TransactionType.Transfer)
                    .Sum(t => t.Amount);

                var transfersOut = transactions
                    .Where(t =>
                        t.FromAccountId == account.Id &&
                        t.Type == TransactionType.Transfer)
                    .Sum(t => t.Amount);

                var creditCardPayments = transactions
                    .Where(t =>
                        t.FromAccountId == account.Id &&
                        t.Purpose ==
                            TransactionPurpose.CreditCardPayment)
                    .Sum(t => t.Amount);

                balance =
                    account.OpeningBalance
                    + accountIncome
                    + deposits
                    - accountExpenses
                    + transfersIn
                    - transfersOut
                    - creditCardPayments;
            }

            result.Add(new AccountSummaryDto
            {
                AccountId = account.Id,

                Name = account.Name,

                AccountType =
                    account.AccountType,

                Balance = balance,

                CreditLimit =
                    account.CreditLimit,

                CreditOutstanding =
                    outstanding,

                AvailableCredit =
                    availableCredit
            });
        }

        return result;
    }

    public async Task<SavingsInvestmentSummaryDto>
        GetSavingsInvestmentSummaryAsync(
            int userId,
            int year,
            int month)
    {
        var startDate =
            new DateTime(year, month, 1);

        var endDate =
            startDate.AddMonths(1);

        var budget =
            await _context.Budgets
                .FirstOrDefaultAsync(b =>
                    b.UserId == userId &&
                    b.Year == year &&
                    b.Month == month);

        var transactions =
            await _context.Transactions
                .Where(t =>
                    t.UserId == userId &&
                    t.TransactionDate >= startDate &&
                    t.TransactionDate < endDate)
                .ToListAsync();

        var savingsAccountIds = await _context.Accounts
    .Where(a =>
        a.UserId == userId &&
        a.AccountType == "SAVINGS" &&
        a.IsActive)
    .Select(a => a.Id)
    .ToListAsync();

        var savings = transactions
            .Where(t =>
                t.Type == TransactionType.Transfer &&
                t.ToAccountId.HasValue &&
                savingsAccountIds.Contains(t.ToAccountId.Value))
            .Sum(t => t.Amount);

        var investmentRecords =
    await _context.Investments
        .Where(i =>
            i.UserId == userId &&
            i.IsActive &&
            i.InvestmentDate >= startDate &&
            i.InvestmentDate < endDate)
        .ToListAsync();

        var investments =
            investmentRecords.Sum(i => i.InvestedAmount);

        var savingsTarget =
            budget?.SavingsTarget ?? 0;

        var investmentTarget =
            budget?.InvestmentTarget ?? 0;

        var savingsProgress =
            savingsTarget == 0
                ? 0
                : Math.Round(
                    savings /
                    savingsTarget *
                    100,
                    2);

        var investmentProgress =
            investmentTarget == 0
                ? 0
                : Math.Round(
                    investments /
                    investmentTarget *
                    100,
                    2);

        return new SavingsInvestmentSummaryDto
        {
            Year = year,

            Month = month,

            SavingsTarget =
                savingsTarget,

            ActualSavings =
                savings,

            SavingsProgress =
                savingsProgress,

            InvestmentTarget =
                investmentTarget,

            ActualInvestment =
                investments,

            InvestmentProgress =
                investmentProgress
        };
    }
}