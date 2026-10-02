using FinanceManager.API.DTOs;

namespace FinanceManager.API.Services.Interfaces;

public interface ITransactionService
{
    Task<TransactionResponseDto> CreateAsync(TransactionCreateDto dto, int userId);
    Task<List<TransactionResponseDto>> GetAllAsync(int userId);
    Task<TransactionResponseDto?> GetByIdAsync(int id, int userId);
    Task<bool> DeleteAsync(int id, int userId);
    Task CreateTransferAsync(TransferCreateDto dto, int userId);
    Task<List<TransactionListDto>> GetFilteredAsync(int userId, TransactionFilterDto filter);
}
