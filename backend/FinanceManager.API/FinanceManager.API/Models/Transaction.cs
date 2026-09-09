namespace FinanceManager.API.Models;

public class Transaction
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int? AccountId { get; set; }

    public int? FromAccountId { get; set; }

    public int? ToAccountId { get; set; }

    public int? CategoryId { get; set; }

    public decimal Amount { get; set; }

    public TransactionType Type { get; set; }

    public string Description { get; set; } = string.Empty;

    public DateTime TransactionDate { get; set; }

    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;

    public Account? Account { get; set; }

    public Account? FromAccount { get; set; }

    public Account? ToAccount { get; set; }

    public Category? Category { get; set; }

    public LoanPayment? LoanPayment { get; set; }

    public TransactionPurpose Purpose { get; set; }
}