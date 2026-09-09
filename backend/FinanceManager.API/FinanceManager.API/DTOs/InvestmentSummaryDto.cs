namespace FinanceManager.API.DTOs;

public class InvestmentSummaryDto
{
    public decimal TotalInvestedAmount { get; set; }
    public decimal TotalCurrentValue { get; set; }
    public decimal TotalProfitLoss { get; set; }
    public decimal TotalProfitLossPercentage { get; set; }
    public int InvestmentCount { get; set; }
}