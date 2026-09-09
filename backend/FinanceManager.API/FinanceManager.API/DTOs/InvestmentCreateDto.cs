using FinanceManager.API.Models;
using System.ComponentModel.DataAnnotations;

namespace FinanceManager.API.DTOs;

public class InvestmentCreateDto
{
    [Range(1, int.MaxValue)]
    public int AccountId { get; set; }

    [Required]
    public string Name { get; set; } = string.Empty;

    [Required]
    public InvestmentType InvestmentType { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }

    public DateTime InvestmentDate { get; set; }

    public string? Description { get; set; }
}