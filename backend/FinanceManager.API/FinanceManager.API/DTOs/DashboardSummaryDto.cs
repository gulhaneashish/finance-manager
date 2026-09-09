namespace FinanceManager.API.DTOs;

public class DashboardSummaryDto
{
    public int Year { get; set; }

    public int Month { get; set; }

    public decimal TotalBalance { get; set; }

    public decimal TotalIncome { get; set; }

    public decimal TotalExpenses { get; set; }

    public decimal ExpenseBudget { get; set; }

    public decimal BudgetSpent { get; set; }

    public decimal BudgetRemaining { get; set; }

    public decimal SavingsTarget { get; set; }
    public decimal ActualSavings { get; set; }

    public decimal InvestmentTarget { get; set; }
    public decimal ActualInvestment { get; set; }
}