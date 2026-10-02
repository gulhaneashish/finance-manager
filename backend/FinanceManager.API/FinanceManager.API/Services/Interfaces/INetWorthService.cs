using FinanceManager.API.DTOs;

namespace FinanceManager.API.Services.Interfaces;

public interface INetWorthService
{
    Task<NetWorthDto> GetNetWorthAsync(int userId);
}
