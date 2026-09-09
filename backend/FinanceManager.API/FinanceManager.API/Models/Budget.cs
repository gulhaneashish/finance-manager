namespace FinanceManager.API.Models;

public class Budget
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int Year { get; set; }

    public int Month { get; set; }

    public decimal ExpectedIncome { get; set; }

    public decimal ExpenseBudget { get; set; }

    public decimal SavingsTarget { get; set; }

    public decimal InvestmentTarget { get; set; }

    public User User { get; set; } = null!;

    public ICollection<CategoryBudget> CategoryBudgets { get; set; }
        = new List<CategoryBudget>();
}