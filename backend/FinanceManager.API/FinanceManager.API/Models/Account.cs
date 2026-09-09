namespace FinanceManager.API.Models;

public class Account
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string AccountType { get; set; } = string.Empty;

    public decimal OpeningBalance { get; set; }

    public decimal? CreditLimit { get; set; }

    public bool IsActive { get; set; } = true;

    public User User { get; set; } = null!;

    public ICollection<Transaction> Transactions { get; set; }
        = new List<Transaction>();

    public ICollection<Transaction> TransfersFrom { get; set; }
        = new List<Transaction>();

    public ICollection<Transaction> TransfersTo { get; set; }
        = new List<Transaction>();
}