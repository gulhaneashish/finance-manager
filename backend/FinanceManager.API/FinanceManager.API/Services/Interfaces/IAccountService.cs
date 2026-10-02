using FinanceManager.API.DTOs;
using FinanceManager.API.Models;

namespace FinanceManager.API.Services.Interfaces;

public interface IAccountService
{
    Task<AccountResponseDto> CreateAsync(AccountCreateDto dto, int userId);
    Task<List<AccountResponseDto>> GetAllAsync(int userId);
    Task<AccountResponseDto?> GetByIdAsync(int id, int userId);
    Task<bool> DeleteAsync(int id, int userId);
    Task<decimal?> GetBalanceAsync(int id, int userId);
    Task<AccountResponseDto?> UpdateAsync(int id, AccountUpdateDto dto, int userId);
    Task<Transaction> AddMoneyAsync(AddMoneyDto dto, int userId);
    Task<List<AccountResponseDto>> GetActiveAccountsAsync(int userId);
}
