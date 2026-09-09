namespace FinanceManager.API.DTOs;

public class MonthlyCashFlowDto
{
    public int Year { get; set; }

    public int Month { get; set; }

    public string MonthName { get; set; }
        = string.Empty;

    public decimal Income { get; set; }

    public decimal Expenses { get; set; }

    public decimal Savings { get; set; }
}