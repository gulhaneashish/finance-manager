namespace FinanceManager.API.DTOs;

public class CategorySpendingDto
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; }
        = string.Empty;

    public decimal Amount { get; set; }

    public decimal Percentage { get; set; }
}