namespace FinanceManager.API.DTOs;

public class ReportSummaryDto
{
    public decimal TotalIncome { get; set; }

    public decimal TotalExpense { get; set; }

    public decimal NetAmount { get; set; }

    public List<CategoryReportsDto> CategoryExpenses { get; set; }
        = new();
}