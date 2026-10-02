using FinanceManager.API.Models;
using Microsoft.EntityFrameworkCore.Storage;
using System.Linq.Expressions;

namespace FinanceManager.API.Repositories.Interfaces;

public interface ILoanPaymentRepository
{
    Task<LoanPayment?> GetByIdAsync(int id);
    Task<List<LoanPayment>> GetAllAsync();
    Task<List<LoanPayment>> FindAsync(Expression<Func<LoanPayment, bool>> predicate);
    Task<LoanPayment?> FirstOrDefaultAsync(Expression<Func<LoanPayment, bool>> predicate);
    Task<bool> AnyAsync(Expression<Func<LoanPayment, bool>> predicate);
    Task<int> CountAsync(Expression<Func<LoanPayment, bool>>? predicate = null);
    IQueryable<LoanPayment> Query();
    Task AddAsync(LoanPayment entity);
    Task AddRangeAsync(IEnumerable<LoanPayment> entities);
    void Update(LoanPayment entity);
    void Remove(LoanPayment entity);
    void RemoveRange(IEnumerable<LoanPayment> entities);
    Task SaveChangesAsync();
    Task<IDbContextTransaction> BeginTransactionAsync();
}
