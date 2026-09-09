using FinanceManager.API.Models;
using System.ComponentModel.DataAnnotations;

namespace FinanceManager.API.DTOs;

public class TransactionCreateDto
{
    [Required]
    public int? AccountId { get; set; }

    public int? FromAccountId { get; set; }

    public int? ToAccountId { get; set; }

    public int? CategoryId { get; set; }

    [Required]
    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required]
    public TransactionType Type { get; set; }

    [MaxLength(250)]
    public string Description { get; set; } = string.Empty;

    public DateTime TransactionDate { get; set; }

    public TransactionPurpose Purpose { get; set; }
}