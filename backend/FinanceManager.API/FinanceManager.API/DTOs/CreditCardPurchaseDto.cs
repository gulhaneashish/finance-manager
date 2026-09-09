using System.ComponentModel.DataAnnotations;

namespace FinanceManager.API.DTOs;

public class CreditCardPurchaseDto
{
    [Required]
    public int AccountId { get; set; }

    [Required]
    public int CategoryId { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    public DateTime TransactionDate { get; set; }

    public string? Description { get; set; }
}