using FinanceManager.API.Data;
using FinanceManager.API.Models;
using FinanceManager.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace FinanceManager.API.Repositories;

public class TransactionRepository : ITransactionRepository
{
    private readonly FinanceDbContext _context;

    public TransactionRepository(FinanceDbContext context)
    {
        _context = context;
    }

    public async Task<Transaction?> GetByIdAsync(int id)
    {
        return await _context.Transactions.FindAsync(id);
    }

    public async Task<List<Transaction>> GetAllAsync()
    {
        return await _context.Transactions.ToListAsync();
    }

    public async Task<List<Transaction>> FindAsync(Expression<Func<Transaction, bool>> predicate)
    {
        return await _context.Transactions.Where(predicate).ToListAsync();
    }

    public async Task<Transaction?> FirstOrDefaultAsync(Expression<Func<Transaction, bool>> predicate)
    {
        return await _context.Transactions.FirstOrDefaultAsync(predicate);
    }

    public async Task<bool> AnyAsync(Expression<Func<Transaction, bool>> predicate)
    {
        return await _context.Transactions.AnyAsync(predicate);
    }

    public async Task<int> CountAsync(Expression<Func<Transaction, bool>>? predicate = null)
    {
        return predicate == null
            ? await _context.Transactions.CountAsync()
            : await _context.Transactions.CountAsync(predicate);
    }

    public IQueryable<Transaction> Query()
    {
        return _context.Transactions.AsQueryable();
    }

    public async Task AddAsync(Transaction entity)
    {
        await _context.Transactions.AddAsync(entity);
    }

    public async Task AddRangeAsync(IEnumerable<Transaction> entities)
    {
        await _context.Transactions.AddRangeAsync(entities);
    }

    public void Update(Transaction entity)
    {
        _context.Transactions.Update(entity);
    }

    public void Remove(Transaction entity)
    {
        _context.Transactions.Remove(entity);
    }

    public void RemoveRange(IEnumerable<Transaction> entities)
    {
        _context.Transactions.RemoveRange(entities);
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
