using FinanceManager.API.DTOs;
using FinanceManager.API.Models;
using FinanceManager.API.Repositories.Interfaces;
using FinanceManager.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.API.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ITransactionRepository _transactionRepository;

    public CategoryService(
        ICategoryRepository categoryRepository,
        ITransactionRepository transactionRepository)
    {
        _categoryRepository = categoryRepository;
        _transactionRepository = transactionRepository;
    }

    public async Task<CategoryResponseDto> CreateAsync(
        CategoryCreateDto dto,
        int userId)
    {
        var existing = await _categoryRepository.FirstOrDefaultAsync(c =>
            c.UserId == userId &&
            c.Name.ToLower() == dto.Name.ToLower() &&
            c.Type == dto.Type);

        if (existing != null)
        {
            throw new InvalidOperationException(
                "Category already exists.");
        }

        var category = new Category
        {
            UserId = userId,
            Name = dto.Name.Trim(),
            Type = dto.Type.ToUpper(),
        };

        await _categoryRepository.AddAsync(category);
        await _categoryRepository.SaveChangesAsync();

        return MapToDto(category);
    }

    public async Task<List<CategoryResponseDto>> GetAllAsync(
        int userId)
    {
        var categories = await _categoryRepository.Query()
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.Type)
            .ThenBy(c => c.Name)
            .ToListAsync();

        return categories
            .Select(MapToDto)
            .ToList();
    }

    public async Task<bool> DeleteAsync(
        int id,
        int userId)
    {
        var category = await _categoryRepository.FirstOrDefaultAsync(c =>
            c.Id == id &&
            c.UserId == userId);

        if (category == null)
        {
            return false;
        }

        var hasTransactions = await _transactionRepository.AnyAsync(t =>
            t.CategoryId == id);

        if (hasTransactions)
        {
            throw new InvalidOperationException(
                "Category cannot be deleted because it is used by transactions.");
        }

        _categoryRepository.Remove(category);
        await _categoryRepository.SaveChangesAsync();

        return true;
    }

    private static CategoryResponseDto MapToDto(
        Category category)
    {
        return new CategoryResponseDto
        {
            Id = category.Id,
            Name = category.Name,
            Type = category.Type
        };
    }
}