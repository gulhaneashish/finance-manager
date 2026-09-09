using System.ComponentModel.DataAnnotations;

namespace FinanceManager.API.DTOs;

public class CategoryCreateDto
{
    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Type { get; set; } = string.Empty;
}