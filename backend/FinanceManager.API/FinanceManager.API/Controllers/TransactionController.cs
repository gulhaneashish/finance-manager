using System.Security.Claims;
using FinanceManager.API.DTOs;
using FinanceManager.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManager.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TransactionController : ControllerBase
{
    private readonly ITransactionService _transactionService;

    public TransactionController(
        ITransactionService transactionService)
    {
        _transactionService = transactionService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        TransactionCreateDto dto)
    {
        var userId = GetUserId();

        var transaction =
            await _transactionService.CreateAsync(
                dto,
                userId);

        return Ok(transaction);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetUserId();

        var transactions =
            await _transactionService.GetAllAsync(userId);

        return Ok(transactions);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = GetUserId();

        var transaction =
            await _transactionService.GetByIdAsync(
                id,
                userId);

        if (transaction == null)
        {
            return NotFound();
        }

        return Ok(transaction);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();

        var deleted =
            await _transactionService.DeleteAsync(
                id,
                userId);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }

    private int GetUserId()
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        return int.Parse(userId!);
    }

    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer(
    TransferCreateDto dto)
    {
        var userId = GetUserId();

        try
        {
            await _transactionService.CreateTransferAsync(
                dto,
                userId);

            return Ok(new
            {
                message = "Transfer completed successfully."
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
    [HttpGet("filter")]
    public async Task<IActionResult> GetFiltered(
    [FromQuery] TransactionFilterDto filter)
    {
        var userId = GetUserId();

        var transactions =
            await _transactionService
                .GetFilteredAsync(
                    userId,
                    filter);

        return Ok(transactions);
    }
}