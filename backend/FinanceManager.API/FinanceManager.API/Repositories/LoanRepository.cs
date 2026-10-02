using FinanceManager.API.Data;
using FinanceManager.API.Models;
using FinanceManager.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace FinanceManager.API.Repositories;

public class LoanRepository : ILoanRepository
{
    private readonly FinanceDbContext _context;

    public LoanRepository(FinanceDbContext context)
    {
        _context = context;
    }

    public async Task<Loan?> GetByIdAsync(int id)
    {
        return await _context.Loans.FindAsync(id);
    }

    public async Task<List<Loan>> GetAllAsync()
    {
        return await _context.Loans.ToListAsync();
    }

    public async Task<List<Loan>> FindAsync(Expression<Func<Loan, bool>> predicate)
    {
        return await _context.Loans.Where(predicate).ToListAsync();
    }

    public async Task<Loan?> FirstOrDefaultAsync(Expression<Func<Loan, bool>> predicate)
    {
        return await _context.Loans.FirstOrDefaultAsync(predicate);
    }

    public async Task<bool> AnyAsync(Expression<Func<Loan, bool>> predicate)
    {
        return await _context.Loans.AnyAsync(predicate);
    }

    public async Task<int> CountAsync(Expression<Func<Loan, bool>>? predicate = null)
    {
        return predicate == null
            ? await _context.Loans.CountAsync()
            : await _context.Loans.CountAsync(predicate);
    }

    public IQueryable<Loan> Query()
    {
        return _context.Loans.AsQueryable();
    }

    public async Task AddAsync(Loan entity)
    {
        await _context.Loans.AddAsync(entity);
    }

    public async Task AddRangeAsync(IEnumerable<Loan> entities)
    {
        await _context.Loans.AddRangeAsync(entities);
    }

    public void Update(Loan entity)
    {
        _context.Loans.Update(entity);
    }

    public void Remove(Loan entity)
    {
        _context.Loans.Remove(entity);
    }

    public void RemoveRange(IEnumerable<Loan> entities)
    {
        _context.Loans.RemoveRange(entities);
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
