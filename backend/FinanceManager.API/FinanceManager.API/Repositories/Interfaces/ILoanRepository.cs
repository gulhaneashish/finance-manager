using FinanceManager.API.Models;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace FinanceManager.API.Repositories.Interfaces;

public interface ILoanRepository
{
    Task<Loan?> GetByIdAsync(int id);
    Task<List<Loan>> GetAllAsync();
    Task<List<Loan>> FindAsync(Expression<Func<Loan, bool>> predicate);
    Task<Loan?> FirstOrDefaultAsync(Expression<Func<Loan, bool>> predicate);
    Task<bool> AnyAsync(Expression<Func<Loan, bool>> predicate);
    Task<int> CountAsync(Expression<Func<Loan, bool>>? predicate = null);
    IQueryable<Loan> Query();
    Task AddAsync(Loan entity);
    Task AddRangeAsync(IEnumerable<Loan> entities);
    void Update(Loan entity);
    void Remove(Loan entity);
    void RemoveRange(IEnumerable<Loan> entities);
    Task SaveChangesAsync();
    Task<IDbContextTransaction> BeginTransactionAsync();
}
