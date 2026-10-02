using FinanceManager.API.Data;
using FinanceManager.API.Models;
using FinanceManager.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace FinanceManager.API.Repositories;

public class InvestmentRepository : IInvestmentRepository
{
    private readonly FinanceDbContext _context;

    public InvestmentRepository(FinanceDbContext context)
    {
        _context = context;
    }

    public async Task<Investment?> GetByIdAsync(int id)
    {
        return await _context.Investments.FindAsync(id);
    }

    public async Task<List<Investment>> GetAllAsync()
    {
        return await _context.Investments.ToListAsync();
    }

    public async Task<List<Investment>> FindAsync(Expression<Func<Investment, bool>> predicate)
    {
        return await _context.Investments.Where(predicate).ToListAsync();
    }

    public async Task<Investment?> FirstOrDefaultAsync(Expression<Func<Investment, bool>> predicate)
    {
        return await _context.Investments.FirstOrDefaultAsync(predicate);
    }

    public async Task<bool> AnyAsync(Expression<Func<Investment, bool>> predicate)
    {
        return await _context.Investments.AnyAsync(predicate);
    }

    public async Task<int> CountAsync(Expression<Func<Investment, bool>>? predicate = null)
    {
        return predicate == null
            ? await _context.Investments.CountAsync()
            : await _context.Investments.CountAsync(predicate);
    }

    public IQueryable<Investment> Query()
    {
        return _context.Investments.AsQueryable();
    }

    public async Task AddAsync(Investment entity)
    {
        await _context.Investments.AddAsync(entity);
    }

    public async Task AddRangeAsync(IEnumerable<Investment> entities)
    {
        await _context.Investments.AddRangeAsync(entities);
    }

    public void Update(Investment entity)
    {
        _context.Investments.Update(entity);
    }

    public void Remove(Investment entity)
    {
        _context.Investments.Remove(entity);
    }

    public void RemoveRange(IEnumerable<Investment> entities)
    {
        _context.Investments.RemoveRange(entities);
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
