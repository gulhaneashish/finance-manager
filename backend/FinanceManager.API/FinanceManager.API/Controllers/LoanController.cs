using System.Security.Claims;
using FinanceManager.API.DTOs;
using FinanceManager.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManager.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class LoanController : ControllerBase
{
    private readonly ILoanService _loanService;

    public LoanController(ILoanService loanService)
    {
        _loanService = loanService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        LoanCreateDto dto)
    {
        var userId = GetUserId();

        var loan = await _loanService.CreateAsync(
            dto,
            userId);

        return Ok(loan);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
    int id,
    LoanUpdateDto dto)
    {
        var userId = GetUserId();

        try
        {
            var loan = await _loanService.UpdateAsync(
                id,
                dto,
                userId);

            return Ok(loan);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetUserId();

        var loans = await _loanService.GetAllAsync(userId);

        return Ok(loans);
    }

    private int GetUserId()
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        return int.Parse(userId!);
    }

    [HttpPost("{id:int}/payments")]
    public async Task<IActionResult> AddPayment(
     int id,
     LoanPaymentCreateDto dto)
    {
        var userId = GetUserId();

        var loan = await _loanService.AddPaymentAsync(
            id,
            dto,
            userId);

        return Ok(loan);
    }
}