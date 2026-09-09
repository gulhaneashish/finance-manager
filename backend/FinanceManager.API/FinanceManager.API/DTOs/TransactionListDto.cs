using FinanceManager.API.Models;

namespace FinanceManager.API.DTOs;

public class TransactionListDto
{
    public int Id { get; set; }

    public int? AccountId { get; set; }

    public string? AccountName { get; set; }

    public int? CategoryId { get; set; }

    public string? CategoryName { get; set; }

    public decimal Amount { get; set; }

    public TransactionType Type { get; set; }

    public string Description { get; set; } = string.Empty;

    public DateTime TransactionDate { get; set; }

    public TransactionPurpose Purpose { get; set; }
}