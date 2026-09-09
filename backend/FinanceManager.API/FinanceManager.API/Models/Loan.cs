using FinanceManager.API.Models;

public class Loan
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public int AccountId { get; set; }

    public int? TransactionId { get; set; }

    public string PersonName { get; set; } = string.Empty;

    public LoanType Type { get; set; }

    public decimal OriginalAmount { get; set; }

    public DateTime LoanDate { get; set; }

    public DateTime? DueDate { get; set; }

    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;

    public User User { get; set; } = null!;

    public Transaction? Transaction { get; set; }

    public ICollection<LoanPayment> Payments { get; set; }
        = new List<LoanPayment>();
}