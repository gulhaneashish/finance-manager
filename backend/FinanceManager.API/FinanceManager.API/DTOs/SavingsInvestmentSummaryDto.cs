namespace FinanceManager.API.DTOs;

public class SavingsInvestmentSummaryDto
{
    public int Year { get; set; }

    public int Month { get; set; }

    public decimal SavingsTarget { get; set; }

    public decimal ActualSavings { get; set; }

    public decimal SavingsProgress { get; set; }

    public decimal InvestmentTarget { get; set; }

    public decimal ActualInvestment { get; set; }

    public decimal InvestmentProgress { get; set; }
}