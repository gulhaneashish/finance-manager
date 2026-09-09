namespace FinanceManager.API.Models;

public class CategoryBudget
{
    public int Id { get; set; }

    public int BudgetId { get; set; }

    public int CategoryId { get; set; }

    public decimal Amount { get; set; }

    public Budget Budget { get; set; } = null!;

    public Category Category { get; set; } = null!;
}