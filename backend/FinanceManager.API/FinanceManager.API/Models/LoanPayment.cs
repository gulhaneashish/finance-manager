namespace FinanceManager.API.Models;

public class LoanPayment
{
    public int Id { get; set; }

    public int LoanId { get; set; }

    public int AccountId { get; set; }

    public int TransactionId { get; set; }

    public decimal Amount { get; set; }

    public DateTime PaymentDate { get; set; }

    public string? Notes { get; set; }

    public Loan Loan { get; set; } = null!;

    public Account Account { get; set; } = null!;

    public Transaction Transaction { get; set; } = null!;
}