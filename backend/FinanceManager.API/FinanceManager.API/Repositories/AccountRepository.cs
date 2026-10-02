using FinanceManager.API.Data;
using FinanceManager.API.Models;
using FinanceManager.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace FinanceManager.API.Repositories;

public class AccountRepository : IAccountRepository
{
    private readonly FinanceDbContext _context;

    public AccountRepository(FinanceDbContext context)
    {
        _context = context;
    }

    public async Task<Account?> GetByIdAsync(int id)
    {
        return await _context.Accounts.FindAsync(id);
    }

    public async Task<List<Account>> GetAllAsync()
    {
        return await _context.Accounts.ToListAsync();
    }

    public async Task<List<Account>> FindAsync(Expression<Func<Account, bool>> predicate)
    {
        return await _context.Accounts.Where(predicate).ToListAsync();
    }

    public async Task<Account?> FirstOrDefaultAsync(Expression<Func<Account, bool>> predicate)
    {
        return await _context.Accounts.FirstOrDefaultAsync(predicate);
    }

    public async Task<bool> AnyAsync(Expression<Func<Account, bool>> predicate)
    {
        return await _context.Accounts.AnyAsync(predicate);
    }

    public async Task<int> CountAsync(Expression<Func<Account, bool>>? predicate = null)
    {
        return predicate == null
            ? await _context.Accounts.CountAsync()
            : await _context.Accounts.CountAsync(predicate);
    }

    public IQueryable<Account> Query()
    {
        return _context.Accounts.AsQueryable();
    }

    public async Task AddAsync(Account entity)
    {
        await _context.Accounts.AddAsync(entity);
    }

    public async Task AddRangeAsync(IEnumerable<Account> entities)
    {
        await _context.Accounts.AddRangeAsync(entities);
    }

    public void Update(Account entity)
    {
        _context.Accounts.Update(entity);
    }

    public void Remove(Account entity)
    {
        _context.Accounts.Remove(entity);
    }

    public void RemoveRange(IEnumerable<Account> entities)
    {
        _context.Accounts.RemoveRange(entities);
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
