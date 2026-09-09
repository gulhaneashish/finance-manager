using FinanceManager.API.Models;

namespace FinanceManager.API.DTOs;

public class TransactionResponseDto
{
    public int Id { get; set; }

    public int? AccountId { get; set; }

    public int? FromAccountId { get; set; }

    public int? ToAccountId { get; set; }

    public int? CategoryId { get; set; }

    public decimal Amount { get; set; }

    public TransactionType Type { get; set; }

    public string Description { get; set; } = string.Empty;

    public DateTime TransactionDate { get; set; }

    public DateTime CreatedAt { get; set; }
}