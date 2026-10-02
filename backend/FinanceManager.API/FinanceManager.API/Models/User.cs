namespace FinanceManager.API.Models;

public class User
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public string? RefreshToken { get; set; }

    public DateTime? RefreshTokenExpiryTime { get; set; }

    public string Role { get; set; } = "User";

    public bool IsActive { get; set; } = true;

    public string? ProfilePictureUrl { get; set; }

    public string? PhoneNumber { get; set; }

    public string? Username { get; set; }

    public DateTime? LastLoginAt { get; set; }

    public ICollection<Account> Accounts { get; set; } = new List<Account>();

    public ICollection<Transaction> Transactions { get; set; }
    = new List<Transaction>();

    public ICollection<Category> Categories { get; set; }
        = new List<Category>();
    public ICollection<Budget> Budgets { get; set; }
    = new List<Budget>();
    public ICollection<Loan> Loans { get; set; }
    = new List<Loan>();
}