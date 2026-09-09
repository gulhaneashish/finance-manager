namespace FinanceManager.API.DTOs;

public class AccountSummaryDto
{
    public int AccountId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string AccountType { get; set; } = string.Empty;

    public decimal Balance { get; set; }

    public decimal? CreditLimit { get; set; }

    public decimal CreditOutstanding { get; set; }

    public decimal AvailableCredit { get; set; }
}