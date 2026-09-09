namespace FinanceManager.API.Models;

public class Investment
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public InvestmentType InvestmentType { get; set; }

    public decimal InvestedAmount { get; set; }

    public decimal CurrentValue { get; set; }

    public DateTime InvestmentDate { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public User User { get; set; } = null!;

    public int? TransactionId { get; set; }

    public Transaction? Transaction { get; set; }
}