using System.ComponentModel.DataAnnotations;

namespace FinanceManager.API.DTOs;

public class BudgetCreateDto
{
    [Range(2000, 2100)]
    public int Year { get; set; }

    [Range(1, 12)]
    public int Month { get; set; }

    [Range(0, double.MaxValue)]
    public decimal ExpectedIncome { get; set; }

    [Range(0, double.MaxValue)]
    public decimal ExpenseBudget { get; set; }

    [Range(0, double.MaxValue)]
    public decimal SavingsTarget { get; set; }

    [Range(0, double.MaxValue)]
    public decimal InvestmentTarget { get; set; }

    public List<CategoryBudgetDto> Categories { get; set; }
        = new();
}