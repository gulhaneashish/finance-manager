using System.ComponentModel.DataAnnotations;

namespace FinanceManager.API.DTOs;

public class LoanPaymentCreateDto
{
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required]
    public int AccountId { get; set; }

    public DateTime PaymentDate { get; set; }

    public string? Notes { get; set; }
}