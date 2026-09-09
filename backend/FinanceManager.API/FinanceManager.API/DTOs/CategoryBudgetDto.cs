using System.ComponentModel.DataAnnotations;

namespace FinanceManager.API.DTOs;

public class CategoryBudgetDto
{
    [Required]
    public int CategoryId { get; set; }

    [Range(0.01, double.MaxValue)]
    public decimal Amount { get; set; }
}