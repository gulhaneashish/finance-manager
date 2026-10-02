using System.Security.Claims;
using FinanceManager.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManager.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class NetWorthController : ControllerBase
{
    private readonly INetWorthService _service;

    public NetWorthController(
        INetWorthService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var userId = GetUserId();

        var result =
            await _service.GetNetWorthAsync(userId);

        return Ok(result);
    }

    private int GetUserId()
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        return int.Parse(userId!);
    }
}