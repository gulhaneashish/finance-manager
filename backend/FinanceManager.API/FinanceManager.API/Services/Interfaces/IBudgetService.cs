using FinanceManager.API.DTOs;

namespace FinanceManager.API.Services.Interfaces;

public interface IBudgetService
{
    Task CreateAsync(BudgetCreateDto dto, int userId);
    Task<BudgetResponseDto?> GetAsync(int year, int month, int userId);
    Task UpdateAsync(int year, int month, BudgetCreateDto dto, int userId);
}
