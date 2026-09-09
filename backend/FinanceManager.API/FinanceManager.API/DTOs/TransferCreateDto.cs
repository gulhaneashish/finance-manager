using FinanceManager.API.Models;
using System.ComponentModel.DataAnnotations;

namespace FinanceManager.API.DTOs;

public class TransferCreateDto
{
    [Required]
    public int FromAccountId { get; set; }

    [Required]
    public int ToAccountId { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }
    public TransactionPurpose TransactionPurpose { get; set; }
    public DateTime TransactionDate { get; set; }

    public string? Description { get; set; }
}