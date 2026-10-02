using FinanceManager.API.Models;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace FinanceManager.API.Repositories.Interfaces;

public interface ICategoryBudgetRepository
{
    Task<CategoryBudget?> GetByIdAsync(int id);
    Task<List<CategoryBudget>> GetAllAsync();
    Task<List<CategoryBudget>> FindAsync(Expression<Func<CategoryBudget, bool>> predicate);
    Task<CategoryBudget?> FirstOrDefaultAsync(Expression<Func<CategoryBudget, bool>> predicate);
    Task<bool> AnyAsync(Expression<Func<CategoryBudget, bool>> predicate);
    Task<int> CountAsync(Expression<Func<CategoryBudget, bool>>? predicate = null);
    IQueryable<CategoryBudget> Query();
    Task AddAsync(CategoryBudget entity);
    Task AddRangeAsync(IEnumerable<CategoryBudget> entities);
    void Update(CategoryBudget entity);
    void Remove(CategoryBudget entity);
    void RemoveRange(IEnumerable<CategoryBudget> entities);
    Task SaveChangesAsync();
    Task<IDbContextTransaction> BeginTransactionAsync();
}
