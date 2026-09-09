namespace FinanceManager.API.DTOs;

public class CategoryReportDto
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = string.Empty;

    public decimal Budget { get; set; }

    public decimal Spent { get; set; }

    public decimal Remaining { get; set; }

    public decimal PercentageUsed { get; set; }
}