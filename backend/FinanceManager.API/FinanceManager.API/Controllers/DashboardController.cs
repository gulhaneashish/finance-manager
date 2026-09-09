using System.Security.Claims;
using FinanceManager.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManager.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly DashboardService _dashboardService;

    public DashboardController(
        DashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary(
        int year,
        int month)
    {
        var userId = GetUserId();

        var summary =
            await _dashboardService.GetSummaryAsync(
                userId,
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
    int year,
    int month)
    {
        var userId = GetUserId();

        if (month < 1 || month > 12)
        {
            return BadRequest(new
            {
                message = "Invalid month."
            });
        }

        var result =
            await _dashboardService
                .GetCategorySpendingAsync(
                    userId,
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
    int year,
    int month)
    {
        var userId = GetUserId();

        if (month < 1 || month > 12)
        {
            return BadRequest(new
            {
                message = "Invalid month."
            });
        }

        var result =
            await _dashboardService
                .GetSavingsInvestmentSummaryAsync(
                    userId,
                    year,
                    month);

        return Ok(result);
    }
}