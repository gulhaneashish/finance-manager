using System.Security.Claims;
using FinanceManager.API.DTOs;
using FinanceManager.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManager.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CreditCardController : ControllerBase
{
    private readonly ICreditCardService _service;

    public CreditCardController(
        ICreditCardService service)
    {
        _service = service;
    }

    [HttpPost("purchase")]
    public async Task<IActionResult> Purchase(
        CreditCardPurchaseDto dto)
    {
        try
        {
            var userId = GetUserId();

            var result =
                await _service.MakePurchaseAsync(
                    dto,
                    userId);

            return Ok(new
            {
                id = result.Id,
                accountId = result.AccountId,
                categoryId = result.CategoryId,
                amount = result.Amount,
                type = result.Type,
                purpose = result.Purpose.ToString(),
                description = result.Description,
                transactionDate = result.TransactionDate
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

    [HttpPost("payment")]
    public async Task<IActionResult> Payment(
        CreditCardPaymentDto dto)
    {
        try
        {
            var userId = GetUserId();

            var result =
                await _service.MakePaymentAsync(
                    dto,
                    userId);

            return Ok(new
            {
                id = result.Id,
                fromAccountId = result.FromAccountId,
                toAccountId = result.ToAccountId,
                amount = result.Amount,
                type = result.Type,
                purpose = result.Purpose.ToString(),
                description = result.Description,
                paymentDate = result.TransactionDate
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
        var userId =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        return int.Parse(userId!);
    }
}