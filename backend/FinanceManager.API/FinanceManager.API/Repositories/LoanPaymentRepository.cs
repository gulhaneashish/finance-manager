using FinanceManager.API.Data;
using FinanceManager.API.Models;
using FinanceManager.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace FinanceManager.API.Repositories;

public class LoanPaymentRepository : ILoanPaymentRepository
{
    private readonly FinanceDbContext _context;

    public LoanPaymentRepository(FinanceDbContext context)
    {
        _context = context;
    }

    public async Task<LoanPayment?> GetByIdAsync(int id)
    {
        return await _context.LoanPayments.FindAsync(id);
    }

    public async Task<List<LoanPayment>> GetAllAsync()
    {
        return await _context.LoanPayments.ToListAsync();
    }

    public async Task<List<LoanPayment>> FindAsync(Expression<Func<LoanPayment, bool>> predicate)
    {
        return await _context.LoanPayments.Where(predicate).ToListAsync();
    }

    public async Task<LoanPayment?> FirstOrDefaultAsync(Expression<Func<LoanPayment, bool>> predicate)
    {
        return await _context.LoanPayments.FirstOrDefaultAsync(predicate);
    }

    public async Task<bool> AnyAsync(Expression<Func<LoanPayment, bool>> predicate)
    {
        return await _context.LoanPayments.AnyAsync(predicate);
    }

    public async Task<int> CountAsync(Expression<Func<LoanPayment, bool>>? predicate = null)
    {
        return predicate == null
            ? await _context.LoanPayments.CountAsync()
            : await _context.LoanPayments.CountAsync(predicate);
    }

    public IQueryable<LoanPayment> Query()
    {
        return _context.LoanPayments.AsQueryable();
    }

    public async Task AddAsync(LoanPayment entity)
    {
        await _context.LoanPayments.AddAsync(entity);
    }

    public async Task AddRangeAsync(IEnumerable<LoanPayment> entities)
    {
        await _context.LoanPayments.AddRangeAsync(entities);
    }

    public void Update(LoanPayment entity)
    {
        _context.LoanPayments.Update(entity);
    }

    public void Remove(LoanPayment entity)
    {
        _context.LoanPayments.Remove(entity);
    }

    public void RemoveRange(IEnumerable<LoanPayment> entities)
    {
        _context.LoanPayments.RemoveRange(entities);
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
