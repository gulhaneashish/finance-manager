using FinanceManager.API.DTOs;

namespace FinanceManager.API.Services.Interfaces;

public interface ILoanService
{
    Task<LoanResponseDto> CreateAsync(LoanCreateDto dto, int userId);
    Task<List<LoanResponseDto>> GetAllAsync(int userId);
    Task<LoanResponseDto> AddPaymentAsync(int loanId, LoanPaymentCreateDto dto, int userId);
    Task<LoanResponseDto> UpdateAsync(int id, LoanUpdateDto dto, int userId);
}
