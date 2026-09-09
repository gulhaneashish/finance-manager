using System.ComponentModel.DataAnnotations;

namespace FinanceManager.API.DTOs;

public class AccountUpdateDto
{
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string AccountType { get; set; } = string.Empty;

    public decimal? CreditLimit { get; set; }

    public bool IsActive { get; set; }
}