using FinanceManager.API.Data;
using FinanceManager.API.Models;
using FinanceManager.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace FinanceManager.API.Repositories;

public class BudgetRepository : IBudgetRepository
{
    private readonly FinanceDbContext _context;

    public BudgetRepository(FinanceDbContext context)
    {
        _context = context;
    }

    public async Task<Budget?> GetByIdAsync(int id)
    {
        return await _context.Budgets.FindAsync(id);
    }

    public async Task<List<Budget>> GetAllAsync()
    {
        return await _context.Budgets.ToListAsync();
    }

    public async Task<List<Budget>> FindAsync(Expression<Func<Budget, bool>> predicate)
    {
        return await _context.Budgets.Where(predicate).ToListAsync();
    }

    public async Task<Budget?> FirstOrDefaultAsync(Expression<Func<Budget, bool>> predicate)
    {
        return await _context.Budgets.FirstOrDefaultAsync(predicate);
    }

    public async Task<bool> AnyAsync(Expression<Func<Budget, bool>> predicate)
    {
        return await _context.Budgets.AnyAsync(predicate);
    }

    public async Task<int> CountAsync(Expression<Func<Budget, bool>>? predicate = null)
    {
        return predicate == null
            ? await _context.Budgets.CountAsync()
            : await _context.Budgets.CountAsync(predicate);
    }

    public IQueryable<Budget> Query()
    {
        return _context.Budgets.AsQueryable();
    }

    public async Task AddAsync(Budget entity)
    {
        await _context.Budgets.AddAsync(entity);
    }

    public async Task AddRangeAsync(IEnumerable<Budget> entities)
    {
        await _context.Budgets.AddRangeAsync(entities);
    }

    public void Update(Budget entity)
    {
        _context.Budgets.Update(entity);
    }

    public void Remove(Budget entity)
    {
        _context.Budgets.Remove(entity);
    }

    public void RemoveRange(IEnumerable<Budget> entities)
    {
        _context.Budgets.RemoveRange(entities);
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
