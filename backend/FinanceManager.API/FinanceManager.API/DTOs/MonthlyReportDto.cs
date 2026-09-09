namespace FinanceManager.API.DTOs;

public class MonthlyReportDto
{
    public int Year { get; set; }

    public int Month { get; set; }

    public decimal TotalIncome { get; set; }

    public decimal TotalExpenses { get; set; }

    public decimal NetCashFlow { get; set; }

    public decimal ExpenseBudget { get; set; }

    public decimal BudgetSpent { get; set; }

    public decimal BudgetRemaining { get; set; }

    public decimal SavingsTarget { get; set; }

    public decimal InvestmentTarget { get; set; }

    public decimal SavingsAmount { get; set; }

    public decimal InvestmentAmount { get; set; }

    public decimal TotalBorrowed { get; set; }

    public decimal TotalLent { get; set; }

    public List<CategoryReportDto> Categories { get; set; }
        = new();
}