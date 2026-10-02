using FinanceManager.API.DTOs;
using FinanceManager.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InvestmentController : ControllerBase
{
    private readonly IInvestmentService _investmentService;

    public InvestmentController(IInvestmentService investmentService)
    {
        _investmentService = investmentService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] InvestmentCreateDto dto)
    {
        try
        {
            var userId = GetUserId();

            await _investmentService.CreateInvestmentAsync(
                dto,
                userId);

            return Ok(new
            {
                message = "Investment created successfully."
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    private int GetUserId()
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(userId, out var id))
        {
            throw new UnauthorizedAccessException(
                "Invalid user.");
        }

        return id;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetUserId();

        var investments =
            await _investmentService.GetInvestmentsAsync(userId);

        return Ok(investments);
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummary()
    {
        var userId = GetUserId();

        var summary =
            await _investmentService.GetSummaryAsync(userId);

        return Ok(summary);
    }

    [HttpPut("{id}/value")]
    public async Task<IActionResult> UpdateCurrentValue(
    int id,
    [FromBody] InvestmentUpdateDto dto)
    {
        try
        {
            var userId = GetUserId();

            await _investmentService.UpdateCurrentValueAsync(
                id,
                userId,
                dto);

            return Ok(new
            {
                message = "Investment value updated successfully."
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var userId = GetUserId();

            await _investmentService.DeleteInvestmentAsync(
               userId,
                id);

            return Ok(new
            {
                message = "Investment cancelled successfully."
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpPost("{id}/sell")]
    public async Task<IActionResult> Sell(
    int id,
    [FromBody] InvestmentSellDto dto)
    {
        try
        {
            var userId = GetUserId();

            await _investmentService.SellInvestmentAsync(
                userId,
                id,   
                dto);

            return Ok(new
            {
                message = "Investment sold successfully."
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }
}