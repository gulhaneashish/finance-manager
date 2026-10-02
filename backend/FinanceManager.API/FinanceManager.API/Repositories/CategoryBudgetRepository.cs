using FinanceManager.API.Data;
using FinanceManager.API.Models;
using FinanceManager.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace FinanceManager.API.Repositories;

public class CategoryBudgetRepository : ICategoryBudgetRepository
{
    private readonly FinanceDbContext _context;

    public CategoryBudgetRepository(FinanceDbContext context)
    {
        _context = context;
    }

    public async Task<CategoryBudget?> GetByIdAsync(int id)
    {
        return await _context.CategoryBudgets.FindAsync(id);
    }

    public async Task<List<CategoryBudget>> GetAllAsync()
    {
        return await _context.CategoryBudgets.ToListAsync();
    }

    public async Task<List<CategoryBudget>> FindAsync(Expression<Func<CategoryBudget, bool>> predicate)
    {
        return await _context.CategoryBudgets.Where(predicate).ToListAsync();
    }

    public async Task<CategoryBudget?> FirstOrDefaultAsync(Expression<Func<CategoryBudget, bool>> predicate)
    {
        return await _context.CategoryBudgets.FirstOrDefaultAsync(predicate);
    }

    public async Task<bool> AnyAsync(Expression<Func<CategoryBudget, bool>> predicate)
    {
        return await _context.CategoryBudgets.AnyAsync(predicate);
    }

    public async Task<int> CountAsync(Expression<Func<CategoryBudget, bool>>? predicate = null)
    {
        return predicate == null
            ? await _context.CategoryBudgets.CountAsync()
            : await _context.CategoryBudgets.CountAsync(predicate);
    }

    public IQueryable<CategoryBudget> Query()
    {
        return _context.CategoryBudgets.AsQueryable();
    }

    public async Task AddAsync(CategoryBudget entity)
    {
        await _context.CategoryBudgets.AddAsync(entity);
    }

    public async Task AddRangeAsync(IEnumerable<CategoryBudget> entities)
    {
        await _context.CategoryBudgets.AddRangeAsync(entities);
    }

    public void Update(CategoryBudget entity)
    {
        _context.CategoryBudgets.Update(entity);
    }

    public void Remove(CategoryBudget entity)
    {
        _context.CategoryBudgets.Remove(entity);
    }

    public void RemoveRange(IEnumerable<CategoryBudget> entities)
    {
        _context.CategoryBudgets.RemoveRange(entities);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return _context.Database.BeginTransactionAsync();
    }
}
