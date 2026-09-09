using System.Security.Claims;
using FinanceManager.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManager.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class MonthlyReportController : ControllerBase
{
    private readonly MonthlyReportService _service;

    public MonthlyReportController(
        MonthlyReportService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get(
        int year,
        int month)
    {
        var userId = GetUserId();

        var report = await _service.GetReportAsync(
            userId,
            year,
            month);

        return Ok(report);
    }

    private int GetUserId()
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        return int.Parse(userId!);
    }
}