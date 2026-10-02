using FinanceManager.API.Data;
using FinanceManager.API.Models;
using FinanceManager.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace FinanceManager.API.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly FinanceDbContext _context;

    public CategoryRepository(FinanceDbContext context)
    {
        _context = context;
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _context.Categories.FindAsync(id);
    }

    public async Task<List<Category>> GetAllAsync()
    {
        return await _context.Categories.ToListAsync();
    }

    public async Task<List<Category>> FindAsync(Expression<Func<Category, bool>> predicate)
    {
        return await _context.Categories.Where(predicate).ToListAsync();
    }

    public async Task<Category?> FirstOrDefaultAsync(Expression<Func<Category, bool>> predicate)
    {
        return await _context.Categories.FirstOrDefaultAsync(predicate);
    }

    public async Task<bool> AnyAsync(Expression<Func<Category, bool>> predicate)
    {
        return await _context.Categories.AnyAsync(predicate);
    }

    public async Task<int> CountAsync(Expression<Func<Category, bool>>? predicate = null)
    {
        return predicate == null
            ? await _context.Categories.CountAsync()
            : await _context.Categories.CountAsync(predicate);
    }

    public IQueryable<Category> Query()
    {
        return _context.Categories.AsQueryable();
    }

    public async Task AddAsync(Category entity)
    {
        await _context.Categories.AddAsync(entity);
    }

    public async Task AddRangeAsync(IEnumerable<Category> entities)
    {
        await _context.Categories.AddRangeAsync(entities);
    }

    public void Update(Category entity)
    {
        _context.Categories.Update(entity);
    }

    public void Remove(Category entity)
    {
        _context.Categories.Remove(entity);
    }

    public void RemoveRange(IEnumerable<Category> entities)
    {
        _context.Categories.RemoveRange(entities);
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
