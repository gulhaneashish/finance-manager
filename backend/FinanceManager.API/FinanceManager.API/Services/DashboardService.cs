using FinanceManager.API.DTOs;
using FinanceManager.API.Models;
using FinanceManager.API.Repositories.Interfaces;
using FinanceManager.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.API.Services;

public class DashboardService : IDashboardService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly IBudgetRepository _budgetRepository;
    private readonly ILoanRepository _loanRepository;
    private readonly IInvestmentRepository _investmentRepository;

    public DashboardService(
        ITransactionRepository transactionRepository,
        IAccountRepository accountRepository,
        IBudgetRepository budgetRepository,
        ILoanRepository loanRepository,
        IInvestmentRepository investmentRepository)
    {
        _transactionRepository = transactionRepository;
        _accountRepository = accountRepository;
        _budgetRepository = budgetRepository;
        _loanRepository = loanRepository;
        _investmentRepository = investmentRepository;
    }

    private (
    DateTime StartDate,
    DateTime EndDateExclusive,
    int Year,
    int Month,
    string Period,
    string PeriodLabel
) ResolveDateRange(
    string? period,
    DateTime? startDate,
    DateTime? endDate,
    int? year,
    int? month)
{
    // Always work with UTC dates when querying PostgreSQL
    // timestamp with time zone columns.
    var today = DateTime.UtcNow.Date;

    if (startDate.HasValue && endDate.HasValue)
    {
        var start = DateTime.SpecifyKind(
            startDate.Value.Date,
            DateTimeKind.Utc);

        var endExclusive = DateTime.SpecifyKind(
            endDate.Value.Date.AddDays(1),
            DateTimeKind.Utc);

        return (
            start,
            endExclusive,
            start.Year,
            start.Month,
            "custom",
            $"{start:MMM dd, yyyy} - {endDate.Value.Date:MMM dd, yyyy}"
        );
    }

    var normalizedPeriod = (period ?? string.Empty)
        .Trim()
        .ToLowerInvariant()
        .Replace("-", "")
        .Replace("_", "");

    switch (normalizedPeriod)
    {
        case "today":
        {
            var start = today;
            var endExclusive = start.AddDays(1);
            var endInclusive = start;

            return (
                start,
                endExclusive,
                start.Year,
                start.Month,
                "today",
                $"Today • {start:MMM dd, yyyy}"
            );
        }

        case "thisweek":
        case "week":
        {
            int diff =
                (7 + (today.DayOfWeek - DayOfWeek.Monday)) % 7;

            var start = today
                .AddDays(-diff)
                .Date;

            var endExclusive = start.AddDays(7);
            var endInclusive = start.AddDays(6);

            return (
                start,
                endExclusive,
                start.Year,
                start.Month,
                "this_week",
                $"This Week • {start:MMM dd} - {endInclusive:MMM dd, yyyy}"
            );
        }

        case "thisyear":
        case "year":
        {
            int y = year ?? today.Year;

            var start = new DateTime(
                y,
                1,
                1,
                0,
                0,
                0,
                DateTimeKind.Utc);

            var endExclusive = new DateTime(
                y + 1,
                1,
                1,
                0,
                0,
                0,
                DateTimeKind.Utc);

            return (
                start,
                endExclusive,
                y,
                1,
                "this_year",
                $"This Year • {y}"
            );
        }

        case "thismonth":
        case "month":
        default:
        {
            int y = year ?? today.Year;
            int m = month ?? today.Month;

            if (m < 1 || m > 12)
            {
                m = today.Month;
            }

            var start = new DateTime(
                y,
                m,
                1,
                0,
                0,
                0,
                DateTimeKind.Utc);

            var endExclusive = start.AddMonths(1);

            return (
                start,
                endExclusive,
                y,
                m,
                "this_month",
                $"This Month • {start:MMMM yyyy}"
            );
        }
    }
}

    public async Task<DashboardSummaryDto> GetSummaryAsync(
        int userId,
        string? period = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int? year = null,
        int? month = null)
    {
        var range = ResolveDateRange(period, startDate, endDate, year, month);

        var transactions = await _transactionRepository.Query()
            .Where(t => t.UserId == userId)
            .ToListAsync();

        var periodTransactions = transactions
            .Where(t =>
                t.TransactionDate >= range.StartDate &&
                t.TransactionDate < range.EndDateExclusive)
            .ToList();

        var income = periodTransactions
            .Where(t =>
                t.Type == TransactionType.Income &&
                t.Purpose != TransactionPurpose.Deposit &&
                t.Purpose != TransactionPurpose.OpeningBalance &&
                t.Description != "Opening balance" &&
                t.Purpose != TransactionPurpose.LoanBorrowed &&
                t.Purpose != TransactionPurpose.LoanReceived &&
                t.Purpose != TransactionPurpose.InvestmentSale &&
                t.Purpose != TransactionPurpose.LoanRepayment)
            .Sum(t => t.Amount);

        var expenses = periodTransactions
            .Where(t =>
                (
                    t.Type == TransactionType.Expense ||
                    t.Type == TransactionType.CreditCard
                ) &&
                t.Purpose != TransactionPurpose.Investment &&
                t.Purpose != TransactionPurpose.LoanLent &&
                t.Purpose != TransactionPurpose.LoanRepayment)
            .Sum(t => t.Amount);

        var accounts = await _accountRepository.Query()
    .Where(a => a.UserId == userId && a.IsActive)
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

                var outstanding = Math.Max(purchases - payments, 0);

                totalBalance -= outstanding;

                continue;
            }

            var accountIncome = transactions
                .Where(t =>
                    t.AccountId == account.Id &&
                    t.Type == TransactionType.Income &&
                    t.Purpose != TransactionPurpose.Deposit &&
                    t.Purpose != TransactionPurpose.OpeningBalance &&
                    t.Description != "Opening balance")
                .Sum(t => t.Amount);

            var deposits = transactions
                .Where(t =>
                    t.AccountId == account.Id &&
                    t.Purpose == TransactionPurpose.Deposit &&
                    t.Purpose != TransactionPurpose.OpeningBalance &&
                    t.Description != "Opening balance")
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

        var savingsAccountIds = await _accountRepository.Query()
            .Where(a =>
                a.UserId == userId &&
                a.AccountType == "SAVINGS" &&
                a.IsActive)
            .Select(a => a.Id)
            .ToListAsync();

        var savingsIn = periodTransactions
            .Where(t =>
                t.Type == TransactionType.Transfer &&
                t.ToAccountId.HasValue &&
                savingsAccountIds.Contains(t.ToAccountId.Value))
            .Sum(t => t.Amount);

        var savingsOut = periodTransactions
            .Where(t =>
                t.Type == TransactionType.Transfer &&
                t.FromAccountId.HasValue &&
                savingsAccountIds.Contains(t.FromAccountId.Value))
            .Sum(t => t.Amount);

        var savings = savingsIn - savingsOut;

        var investments = await _investmentRepository.Query()
            .Where(i =>
                i.UserId == userId &&
                i.IsActive &&
                i.InvestmentDate >= range.StartDate &&
                i.InvestmentDate < range.EndDateExclusive)
            .SumAsync(i => (decimal?)i.InvestedAmount) ?? 0;

        decimal budgetAmount = 0;
        decimal savingsTarget = 0;
        decimal investmentTarget = 0;

        if (range.Period == "this_year")
        {
            var yearBudgets = await _budgetRepository.Query()
                .Where(b => b.UserId == userId && b.Year == range.Year)
                .ToListAsync();
            budgetAmount = yearBudgets.Sum(b => b.ExpenseBudget);
            savingsTarget = yearBudgets.Sum(b => b.SavingsTarget);
            investmentTarget = yearBudgets.Sum(b => b.InvestmentTarget);
        }
        else
        {
            var startYear = range.StartDate.Year;
            var startMonth = range.StartDate.Month;
            var endLastDay = range.EndDateExclusive.AddDays(-1);
            var endYear = endLastDay.Year;
            var endMonth = endLastDay.Month;

            var relevantBudgets = await _budgetRepository.Query()
                .Where(b => b.UserId == userId &&
                    ((b.Year == startYear && b.Month == startMonth) ||
                     (b.Year == endYear && b.Month == endMonth)))
                .ToListAsync();

            budgetAmount = relevantBudgets.Sum(b => b.ExpenseBudget);
            savingsTarget = relevantBudgets.Sum(b => b.SavingsTarget);
            investmentTarget = relevantBudgets.Sum(b => b.InvestmentTarget);
        }

        return new DashboardSummaryDto
        {
            Year = range.Year,
            Month = range.Month,
            StartDate = range.StartDate,
            EndDate = range.EndDateExclusive.AddDays(-1),
            Period = range.Period,
            PeriodLabel = range.PeriodLabel,
            TotalBalance = totalBalance,
            TotalIncome = income,
            TotalExpenses = expenses,
            ExpenseBudget = budgetAmount,
            BudgetSpent = expenses,
            BudgetRemaining = budgetAmount - expenses,
            SavingsTarget = savingsTarget,
            ActualSavings = savings,
            InvestmentTarget = investmentTarget,
            ActualInvestment = investments
        };
    }

    public async Task<List<CategorySpendingDto>> GetCategorySpendingAsync(
        int userId,
        string? period = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int? year = null,
        int? month = null)
    {
        var range = ResolveDateRange(period, startDate, endDate, year, month);

        var expenses = await _transactionRepository.Query()
            .Include(t => t.Category)
            .Where(t =>
                t.UserId == userId &&
                (
                    t.Type == TransactionType.Expense ||
                    t.Type == TransactionType.CreditCard
                ) &&
                t.TransactionDate >= range.StartDate &&
                t.TransactionDate < range.EndDateExclusive &&
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

    public async Task<List<MonthlyCashFlowDto>> GetMonthlyCashFlowAsync(
    int userId,
    int year)
{
    var startDate = new DateTime(
        year,
        1,
        1,
        0,
        0,
        0,
        DateTimeKind.Utc);

    var endDate = startDate.AddYears(1);

    var transactions = await _transactionRepository.Query()
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
                    t.Purpose != TransactionPurpose.OpeningBalance &&
                    t.Description != "Opening balance" &&
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
                MonthName = new DateTime(year, month, 1)
                    .ToString("MMM"),
                Income = income,
                Expenses = expenses,
                Savings = income - expenses
            };
        })
        .ToList();
}

    public async Task<LoanDebtSummaryDto> GetLoanDebtSummaryAsync(int userId)
    {
        var loans = await _loanRepository.Query()
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
                        loan.OriginalAmount - paidAmount,
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
                    OriginalAmount = loan.OriginalAmount,
                    PaidAmount = paidAmount,
                    RemainingAmount = remainingAmount,
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
            NetDebt = totalOwedByMe - totalOwedToMe,
            Loans = loanItems
        };
    }

    public async Task<List<AccountSummaryDto>> GetAccountSummaryAsync(int userId)
    {
        var accounts = await _accountRepository.Query()
            .Where(a =>
                a.UserId == userId &&
                a.IsActive)
            .ToListAsync();

        var transactions = await _transactionRepository.Query()
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
                        t.Purpose == TransactionPurpose.CreditCardPurchase)
                    .Sum(t => t.Amount);

                var payments = transactions
                    .Where(t =>
                        t.ToAccountId == account.Id &&
                        t.Purpose == TransactionPurpose.CreditCardPayment)
                    .Sum(t => t.Amount);

                outstanding = Math.Max(purchases - payments, 0);
                balance = -outstanding;

                if (account.CreditLimit.HasValue)
                {
                    availableCredit = Math.Max(
                        account.CreditLimit.Value - outstanding,
                        0);
                }
            }
            else
            {
                var accountIncome = transactions
                    .Where(t =>
                        t.AccountId == account.Id &&
                        t.Type == TransactionType.Income &&
                        t.Purpose != TransactionPurpose.Deposit &&
                        t.Purpose != TransactionPurpose.OpeningBalance &&
                        t.Description != "Opening balance")
                    .Sum(t => t.Amount);

                var deposits = transactions
                    .Where(t =>
                        t.AccountId == account.Id &&
                        t.Purpose == TransactionPurpose.Deposit &&
                        t.Purpose != TransactionPurpose.OpeningBalance &&
                        t.Description != "Opening balance")
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
                AccountType = account.AccountType,
                Balance = balance,
                CreditLimit = account.CreditLimit,
                CreditOutstanding = outstanding,
                AvailableCredit = availableCredit
            });
        }

        return result;
    }

    public async Task<SavingsInvestmentSummaryDto> GetSavingsInvestmentSummaryAsync(
        int userId,
        string? period = null,
        DateTime? startDate = null,
        DateTime? endDate = null,
        int? year = null,
        int? month = null)
    {
        var range = ResolveDateRange(period, startDate, endDate, year, month);

        decimal savingsTarget = 0;
        decimal investmentTarget = 0;

        if (range.Period == "this_year")
        {
            var yearBudgets = await _budgetRepository.Query()
                .Where(b => b.UserId == userId && b.Year == range.Year)
                .ToListAsync();
            savingsTarget = yearBudgets.Sum(b => b.SavingsTarget);
            investmentTarget = yearBudgets.Sum(b => b.InvestmentTarget);
        }
        else
        {
            var startYear = range.StartDate.Year;
            var startMonth = range.StartDate.Month;
            var endLastDay = range.EndDateExclusive.AddDays(-1);
            var endYear = endLastDay.Year;
            var endMonth = endLastDay.Month;

            var relevantBudgets = await _budgetRepository.Query()
                .Where(b => b.UserId == userId &&
                    ((b.Year == startYear && b.Month == startMonth) ||
                     (b.Year == endYear && b.Month == endMonth)))
                .ToListAsync();

            savingsTarget = relevantBudgets.Sum(b => b.SavingsTarget);
            investmentTarget = relevantBudgets.Sum(b => b.InvestmentTarget);
        }

        var transactions = await _transactionRepository.Query()
            .Where(t =>
                t.UserId == userId &&
                t.TransactionDate >= range.StartDate &&
                t.TransactionDate < range.EndDateExclusive)
            .ToListAsync();

        var savingsAccountIds = await _accountRepository.Query()
            .Where(a =>
                a.UserId == userId &&
                a.AccountType == "SAVINGS" &&
                a.IsActive)
            .Select(a => a.Id)
            .ToListAsync();

        var savingsIn = transactions
            .Where(t =>
                t.Type == TransactionType.Transfer &&
                t.ToAccountId.HasValue &&
                savingsAccountIds.Contains(t.ToAccountId.Value))
            .Sum(t => t.Amount);

        var savingsOut = transactions
            .Where(t =>
                t.Type == TransactionType.Transfer &&
                t.FromAccountId.HasValue &&
                savingsAccountIds.Contains(t.FromAccountId.Value))
            .Sum(t => t.Amount);

        var savings = savingsIn - savingsOut;

        var investmentRecords = await _investmentRepository.Query()
            .Where(i =>
                i.UserId == userId &&
                i.IsActive &&
                i.InvestmentDate >= range.StartDate &&
                i.InvestmentDate < range.EndDateExclusive)
            .ToListAsync();

        var investments =
            investmentRecords.Sum(i => i.InvestedAmount);

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
            Year = range.Year,
            Month = range.Month,
            SavingsTarget = savingsTarget,
            ActualSavings = savings,
            SavingsProgress = savingsProgress,
            InvestmentTarget = investmentTarget,
            ActualInvestment = investments,
            InvestmentProgress = investmentProgress
        };
    }
}