using System.Security.Claims;
using FinanceManager.API.DTOs;
using FinanceManager.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManager.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    // =========================
    // CREATE ACCOUNT
    // =========================

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] AccountCreateDto dto)
    {
        var userId = GetUserId();

        try
        {
            var account = await _accountService.CreateAsync(
                dto,
                userId);

            return Ok(account);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }


    // =========================
    // GET ALL ACCOUNTS
    // =========================

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetUserId();

        var accounts = await _accountService.GetAllAsync(
            userId);

        return Ok(accounts);
    }


    // =========================
    // GET ACCOUNT BY ID
    // =========================

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var userId = GetUserId();

        var account = await _accountService.GetByIdAsync(
            id,
            userId);

        if (account == null)
        {
            return NotFound(new
            {
                message = "Account not found."
            });
        }

        return Ok(account);
    }


    // =========================
    // UPDATE ACCOUNT
    // =========================

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] AccountUpdateDto dto)
    {
        var userId = GetUserId();

        try
        {
            var account = await _accountService.UpdateAsync(
                id,
                dto,
                userId);

            if (account == null)
            {
                return NotFound(new
                {
                    message = "Account not found."
                });
            }

            return Ok(account);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }


    // =========================
    // DEACTIVATE ACCOUNT
    // =========================

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();

        var deleted = await _accountService.DeleteAsync(
            id,
            userId);

        if (!deleted)
        {
            return NotFound(new
            {
                message = "Account not found."
            });
        }

        return NoContent();
    }


    // =========================
    // GET ACCOUNT BALANCE
    // =========================

    [HttpGet("{id:int}/balance")]
    public async Task<IActionResult> GetBalance(int id)
    {
        var userId = GetUserId();

        var balance = await _accountService.GetBalanceAsync(
            id,
            userId);

        if (balance == null)
        {
            return NotFound(new
            {
                message = "Account not found."
            });
        }

        return Ok(new
        {
            accountId = id,
            balance = balance
        });
    }


    // =========================
    // ADD MONEY
    // =========================

    [HttpPost("{accountId:int}/add-money")]
    public async Task<IActionResult> AddMoney(
        int accountId,
        [FromBody] AddMoneyDto dto)
    {
        var userId = GetUserId();

        try
        {
            // Account ID comes from URL.
            // We don't trust the client to send another ID.
            dto.AccountId = accountId;

            var transaction =
                await _accountService.AddMoneyAsync(
                    dto,
                    userId);

            return Ok(new
            {
                message = "Money added successfully.",
                transactionId = transaction.Id,
                accountId = transaction.AccountId,
                amount = transaction.Amount,
                type = transaction.Type,
                purpose = transaction.Purpose,
                description = transaction.Description,
                transactionDate = transaction.TransactionDate
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


    // =========================
    // GET USER ID
    // =========================

    private int GetUserId()
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            throw new UnauthorizedAccessException(
                "User ID not found.");
        }

        return int.Parse(userId);
    }

    [HttpGet("active")]
    public async Task<IActionResult> GetActive()
    {
        var userId = GetUserId();

        var accounts =
            await _accountService.GetActiveAccountsAsync(userId);

        return Ok(accounts);
    }
}