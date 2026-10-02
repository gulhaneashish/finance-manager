using FinanceManager.API.Models;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace FinanceManager.API.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<List<User>> GetAllAsync();
    Task<List<User>> FindAsync(Expression<Func<User, bool>> predicate);
    Task<User?> FirstOrDefaultAsync(Expression<Func<User, bool>> predicate);
    Task<bool> AnyAsync(Expression<Func<User, bool>> predicate);
    Task<int> CountAsync(Expression<Func<User, bool>>? predicate = null);
    IQueryable<User> Query();
    Task AddAsync(User user);
    Task AddRangeAsync(IEnumerable<User> users);
    void Update(User user);
    void Remove(User user);
    void RemoveRange(IEnumerable<User> users);
    Task SaveChangesAsync();
    Task<IDbContextTransaction> BeginTransactionAsync();
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByRefreshTokenAsync(string refreshToken);
}
