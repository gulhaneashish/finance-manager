using System.Security.Claims;
using FinanceManager.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManager.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(
        IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(
        [FromQuery] string? period = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int? year = null,
        [FromQuery] int? month = null)
    {
        var userId = GetUserId();

        var summary =
            await _dashboardService.GetSummaryAsync(
                userId,
                period,
                startDate,
                endDate,
                year,
                month);

        return Ok(summary);
    }

    private int GetUserId()
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        return int.Parse(userId!);
    }

    [HttpGet("category-spending")]
    public async Task<IActionResult> GetCategorySpending(
        [FromQuery] string? period = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int? year = null,
        [FromQuery] int? month = null)
    {
        var userId = GetUserId();

        var result =
            await _dashboardService
                .GetCategorySpendingAsync(
                    userId,
                    period,
                    startDate,
                    endDate,
                    year,
                    month);

        return Ok(result);
    }
    [HttpGet("monthly-cash-flow")]
    public async Task<IActionResult> GetMonthlyCashFlow(
    int year)
    {
        var userId = GetUserId();

        var result =
            await _dashboardService
                .GetMonthlyCashFlowAsync(
                    userId,
                    year);

        return Ok(result);
    }

    [HttpGet("loan-debt")]
    public async Task<IActionResult> GetLoanDebt()
    {
        var userId = GetUserId();

        var result =
            await _dashboardService
                .GetLoanDebtSummaryAsync(userId);

        return Ok(result);
    }
    [HttpGet("accounts")]
    public async Task<IActionResult> GetAccounts()
    {
        var userId = GetUserId();

        var result =
            await _dashboardService
                .GetAccountSummaryAsync(userId);

        return Ok(result);
    }

    [HttpGet("savings-investments")]
    public async Task<IActionResult> GetSavingsInvestments(
        [FromQuery] string? period = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int? year = null,
        [FromQuery] int? month = null)
    {
        var userId = GetUserId();

        var result =
            await _dashboardService
                .GetSavingsInvestmentSummaryAsync(
                    userId,
                    period,
                    startDate,
                    endDate,
                    year,
                    month);

        return Ok(result);
    }
}