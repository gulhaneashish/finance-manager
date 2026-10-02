using FinanceManager.API.Models;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace FinanceManager.API.Repositories.Interfaces;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(int id);
    Task<List<Account>> GetAllAsync();
    Task<List<Account>> FindAsync(Expression<Func<Account, bool>> predicate);
    Task<Account?> FirstOrDefaultAsync(Expression<Func<Account, bool>> predicate);
    Task<bool> AnyAsync(Expression<Func<Account, bool>> predicate);
    Task<int> CountAsync(Expression<Func<Account, bool>>? predicate = null);
    IQueryable<Account> Query();
    Task AddAsync(Account entity);
    Task AddRangeAsync(IEnumerable<Account> entities);
    void Update(Account entity);
    void Remove(Account entity);
    void RemoveRange(IEnumerable<Account> entities);
    Task SaveChangesAsync();
    Task<IDbContextTransaction> BeginTransactionAsync();
}
