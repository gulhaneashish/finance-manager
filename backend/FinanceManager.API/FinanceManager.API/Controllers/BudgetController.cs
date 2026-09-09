using System.Security.Claims;
using FinanceManager.API.DTOs;
using FinanceManager.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManager.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class BudgetController : ControllerBase
{
    private readonly BudgetService _budgetService;

    public BudgetController(BudgetService budgetService)
    {
        _budgetService = budgetService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        BudgetCreateDto dto)
    {
        var userId = GetUserId();

        try
        {
            await _budgetService.CreateAsync(
                dto,
                userId);

            return Ok(new
            {
                message = "Budget created successfully."
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


    [HttpGet("{year:int}/{month:int}")]
    public async Task<IActionResult> Get(
        int year,
        int month)
    {
        var userId = GetUserId();

        var budget = await _budgetService.GetAsync(
            year,
            month,
            userId);

        if (budget == null)
        {
            return NotFound();
        }

        return Ok(budget);
    }

    private int GetUserId()
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        return int.Parse(userId!);
    }

    [HttpPut("{year:int}/{month:int}")]
    public async Task<IActionResult> Update(
    int year,
    int month,
    BudgetCreateDto dto)
    {
        var userId = GetUserId();

        try
        {
            await _budgetService.UpdateAsync(
                year,
                month,
                dto,
                userId);

            return Ok(new
            {
                message = "Budget updated successfully."
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