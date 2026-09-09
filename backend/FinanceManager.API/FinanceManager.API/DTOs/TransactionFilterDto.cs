using FinanceManager.API.Models;

namespace FinanceManager.API.DTOs;

public class TransactionFilterDto
{
    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }

    public int? AccountId { get; set; }

    public int? CategoryId { get; set; }

    public TransactionType? Type { get; set; }
}