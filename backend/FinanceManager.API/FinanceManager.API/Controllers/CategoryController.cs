using System.Security.Claims;
using FinanceManager.API.DTOs;
using FinanceManager.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManager.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly CategoryService _categoryService;

    public CategoryController(CategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        CategoryCreateDto dto)
    {
        var userId = GetUserId();

        var category = await _categoryService.CreateAsync(
            dto,
            userId);

        return Ok(category);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var userId = GetUserId();

        var categories =
            await _categoryService.GetAllAsync(userId);

        return Ok(categories);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetUserId();

        var deleted =
            await _categoryService.DeleteAsync(
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
}