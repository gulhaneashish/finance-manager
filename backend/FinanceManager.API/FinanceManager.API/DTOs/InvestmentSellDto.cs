using System.ComponentModel.DataAnnotations;

namespace FinanceManager.API.DTOs;

public class InvestmentSellDto
{
    [Range(1, int.MaxValue)]
    public int AccountId { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal SellAmount { get; set; }

    public DateTime SellDate { get; set; }

    public string? Description { get; set; }
}