using FinanceManager.API.Models;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace FinanceManager.API.Repositories.Interfaces;

public interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(int id);
    Task<List<Transaction>> GetAllAsync();
    Task<List<Transaction>> FindAsync(Expression<Func<Transaction, bool>> predicate);
    Task<Transaction?> FirstOrDefaultAsync(Expression<Func<Transaction, bool>> predicate);
    Task<bool> AnyAsync(Expression<Func<Transaction, bool>> predicate);
    Task<int> CountAsync(Expression<Func<Transaction, bool>>? predicate = null);
    IQueryable<Transaction> Query();
    Task AddAsync(Transaction entity);
    Task AddRangeAsync(IEnumerable<Transaction> entities);
    void Update(Transaction entity);
    void Remove(Transaction entity);
    void RemoveRange(IEnumerable<Transaction> entities);
    Task SaveChangesAsync();
    Task<IDbContextTransaction> BeginTransactionAsync();
}
