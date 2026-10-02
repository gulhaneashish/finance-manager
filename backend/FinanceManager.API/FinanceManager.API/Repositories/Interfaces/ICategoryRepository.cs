using FinanceManager.API.Models;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace FinanceManager.API.Repositories.Interfaces;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(int id);
    Task<List<Category>> GetAllAsync();
    Task<List<Category>> FindAsync(Expression<Func<Category, bool>> predicate);
    Task<Category?> FirstOrDefaultAsync(Expression<Func<Category, bool>> predicate);
    Task<bool> AnyAsync(Expression<Func<Category, bool>> predicate);
    Task<int> CountAsync(Expression<Func<Category, bool>>? predicate = null);
    IQueryable<Category> Query();
    Task AddAsync(Category entity);
    Task AddRangeAsync(IEnumerable<Category> entities);
    void Update(Category entity);
    void Remove(Category entity);
    void RemoveRange(IEnumerable<Category> entities);
    Task SaveChangesAsync();
    Task<IDbContextTransaction> BeginTransactionAsync();
}
