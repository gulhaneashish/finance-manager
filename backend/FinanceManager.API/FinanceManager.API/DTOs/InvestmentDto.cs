using FinanceManager.API.Models;

namespace FinanceManager.API.DTOs;

public class InvestmentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public InvestmentType InvestmentType { get; set; }
    public decimal InvestedAmount { get; set; }
    public decimal CurrentValue { get; set; }
    public decimal ProfitLoss { get; set; }
    public decimal ProfitLossPercentage { get; set; }
    public DateTime InvestmentDate { get; set; }
    public bool IsActive { get; set; }
}