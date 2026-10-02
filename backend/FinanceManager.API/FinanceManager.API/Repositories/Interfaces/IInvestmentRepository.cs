using FinanceManager.API.Models;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace FinanceManager.API.Repositories.Interfaces;

public interface IInvestmentRepository
{
    Task<Investment?> GetByIdAsync(int id);
    Task<List<Investment>> GetAllAsync();
    Task<List<Investment>> FindAsync(Expression<Func<Investment, bool>> predicate);
    Task<Investment?> FirstOrDefaultAsync(Expression<Func<Investment, bool>> predicate);
    Task<bool> AnyAsync(Expression<Func<Investment, bool>> predicate);
    Task<int> CountAsync(Expression<Func<Investment, bool>>? predicate = null);
    IQueryable<Investment> Query();
    Task AddAsync(Investment entity);
    Task AddRangeAsync(IEnumerable<Investment> entities);
    void Update(Investment entity);
    void Remove(Investment entity);
    void RemoveRange(IEnumerable<Investment> entities);
    Task SaveChangesAsync();
    Task<IDbContextTransaction> BeginTransactionAsync();
}
