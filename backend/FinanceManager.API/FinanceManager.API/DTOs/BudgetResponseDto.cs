namespace FinanceManager.API.DTOs;

public class BudgetResponseDto
{
    public int Id { get; set; }

    public int Year { get; set; }

    public int Month { get; set; }

    public decimal Income { get; set; }

    public decimal ExpenseBudget { get; set; }

    public decimal SavingsTarget { get; set; }

    public decimal InvestmentTarget { get; set; }

    public decimal Spent { get; set; }

    public decimal Remaining { get; set; }

    public List<BudgetCategoryResponseDto> Categories { get; set; }
        = new();
}