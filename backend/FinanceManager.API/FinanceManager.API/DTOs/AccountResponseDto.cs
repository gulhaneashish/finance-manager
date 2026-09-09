namespace FinanceManager.API.DTOs;

public class AccountResponseDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string AccountType { get; set; } = string.Empty;

    public decimal OpeningBalance { get; set; }

    public decimal CurrentBalance { get; set; }

    public decimal? CreditLimit { get; set; }

    public bool IsActive { get; set; }
}