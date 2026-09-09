using FinanceManager.API.Models;
using System.ComponentModel.DataAnnotations;

namespace FinanceManager.API.DTOs;

public class LoanUpdateDto
{
    [Required]
    [MaxLength(100)]
    public string PersonName { get; set; } = string.Empty;

    [Required]
    public LoanType Type { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal OriginalAmount { get; set; }

    public DateTime LoanDate { get; set; }

    public DateTime? DueDate { get; set; }

    public string? Notes { get; set; }

    public int AccountId { get; set; }
}