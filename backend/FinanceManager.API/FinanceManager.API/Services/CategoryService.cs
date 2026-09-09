using FinanceManager.API.Data;
using FinanceManager.API.DTOs;
using FinanceManager.API.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceManager.API.Services;

public class CategoryService
{
    private readonly FinanceDbContext _context;

    public CategoryService(FinanceDbContext context)
    {
        _context = context;
    }

    public async Task<CategoryResponseDto> CreateAsync(
        CategoryCreateDto dto,
        int userId)
    {
        var existing = await _context.Categories
            .FirstOrDefaultAsync(c =>
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

        _context.Categories.Add(category);

        await _context.SaveChangesAsync();

        return MapToDto(category);
    }

    public async Task<List<CategoryResponseDto>> GetAllAsync(
        int userId)
    {
        var categories = await _context.Categories
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
        var category = await _context.Categories
            .FirstOrDefaultAsync(c =>
                c.Id == id &&
                c.UserId == userId);

        if (category == null)
        {
            return false;
        }

        var hasTransactions = await _context.Transactions
            .AnyAsync(t => t.CategoryId == id);

        if (hasTransactions)
        {
            throw new InvalidOperationException(
                "Category cannot be deleted because it is used by transactions.");
        }

        _context.Categories.Remove(category);

        await _context.SaveChangesAsync();

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