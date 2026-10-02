using FinanceManager.API.DTOs;

namespace FinanceManager.API.Services.Interfaces;

public interface ICategoryService
{
    Task<CategoryResponseDto> CreateAsync(CategoryCreateDto dto, int userId);
    Task<List<CategoryResponseDto>> GetAllAsync(int userId);
    Task<bool> DeleteAsync(int id, int userId);
}
