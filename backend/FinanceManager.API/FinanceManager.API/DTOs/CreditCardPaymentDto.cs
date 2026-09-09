using System.ComponentModel.DataAnnotations;

namespace FinanceManager.API.DTOs;

public class CreditCardPaymentDto
{
    [Required]
    public int FromAccountId { get; set; }

    [Required]
    public int CreditCardAccountId { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    public DateTime PaymentDate { get; set; }

    public string? Description { get; set; }
}