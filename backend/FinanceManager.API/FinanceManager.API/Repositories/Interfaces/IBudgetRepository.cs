using FinanceManager.API.Models;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace FinanceManager.API.Repositories.Interfaces;

public interface IBudgetRepository
{
    Task<Budget?> GetByIdAsync(int id);
    Task<List<Budget>> GetAllAsync();
    Task<List<Budget>> FindAsync(Expression<Func<Budget, bool>> predicate);
    Task<Budget?> FirstOrDefaultAsync(Expression<Func<Budget, bool>> predicate);
    Task<bool> AnyAsync(Expression<Func<Budget, bool>> predicate);
    Task<int> CountAsync(Expression<Func<Budget, bool>>? predicate = null);
    IQueryable<Budget> Query();
    Task AddAsync(Budget entity);
    Task AddRangeAsync(IEnumerable<Budget> entities);
    void Update(Budget entity);
    void Remove(Budget entity);
    void RemoveRange(IEnumerable<Budget> entities);
    Task SaveChangesAsync();
    Task<IDbContextTransaction> BeginTransactionAsync();
}
