namespace FinanceManager.API.DTOs;

public class AddMoneyDto
{
    public int AccountId { get; set; }

    public decimal Amount { get; set; }

    public string? Description { get; set; }

    public DateTime TransactionDate { get; set; }
}