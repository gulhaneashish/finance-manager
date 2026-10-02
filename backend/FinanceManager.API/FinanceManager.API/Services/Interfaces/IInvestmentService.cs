using FinanceManager.API.DTOs;

namespace FinanceManager.API.Services.Interfaces;

public interface IInvestmentService
{
    Task CreateInvestmentAsync(InvestmentCreateDto dto, int userId);
    Task<List<InvestmentDto>> GetInvestmentsAsync(int userId);
    Task<InvestmentSummaryDto> GetSummaryAsync(int userId);
    Task UpdateCurrentValueAsync(int id, int userId, InvestmentUpdateDto dto);
    Task<string> DeleteInvestmentAsync(int userId, int id);
    Task<string> SellInvestmentAsync(int userId, int id, InvestmentSellDto dto);
}
