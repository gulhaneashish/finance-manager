using System.ComponentModel.DataAnnotations;

namespace FinanceManager.API.DTOs;

public class AccountCreateDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string AccountType { get; set; } = string.Empty;

    public decimal OpeningBalance { get; set; }

    public decimal? CreditLimit { get; set; }
}