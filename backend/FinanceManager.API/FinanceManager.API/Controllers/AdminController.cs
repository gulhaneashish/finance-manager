using System.Security.Claims;
using FinanceManager.API.DTOs;
using FinanceManager.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManager.API.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/[controller]")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    // ==========================================
    // 1. MANAGE USERS & STATUS
    // ==========================================
    [HttpGet("users")]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _adminService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpPut("users/{id:int}/status")]
    public async Task<IActionResult> UpdateUserStatus(int id, [FromBody] UpdateUserStatusDto dto)
    {
        var adminId = GetUserId();
        var adminEmail = GetUserEmail();

        var success = await _adminService.UpdateUserStatusAsync(id, dto.IsActive, adminId, adminEmail);
        if (!success)
        {
            return NotFound(new { message = "User not found." });
        }

        return Ok(new { message = $"User status updated to {(dto.IsActive ? "active" : "deactivated")}." });
    }

    [HttpPut("users/{id:int}/role")]
    public async Task<IActionResult> UpdateUserRole(int id, [FromBody] UpdateUserRoleDto dto)
    {
        var adminId = GetUserId();
        var adminEmail = GetUserEmail();

        var success = await _adminService.UpdateUserRoleAsync(id, dto.Role, adminId, adminEmail);
        if (!success)
        {
            return NotFound(new { message = "User not found." });
        }

        return Ok(new { message = $"User role updated to {dto.Role}." });
    }

    // ==========================================
    // 2. MANAGE EXPENSE CATEGORIES
    // ==========================================
    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _adminService.GetAllCategoriesAsync();
        return Ok(categories);
    }

    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory([FromBody] AdminCreateCategoryDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest(new { message = "Category name cannot be empty." });
        }

        var adminId = GetUserId();
        var adminEmail = GetUserEmail();

        var created = await _adminService.CreateCategoryAsync(dto, adminId, adminEmail);
        return CreatedAtAction(nameof(GetCategories), new { id = created.Id }, created);
    }

    [HttpPut("categories/{id:int}")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] AdminUpdateCategoryDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Name))
        {
            return BadRequest(new { message = "Category name cannot be empty." });
        }

        var adminId = GetUserId();
        var adminEmail = GetUserEmail();

        var updated = await _adminService.UpdateCategoryAsync(id, dto, adminId, adminEmail);
        if (updated == null)
        {
            return NotFound(new { message = "Category not found." });
        }

        return Ok(updated);
    }

    [HttpDelete("categories/{id:int}")]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        var adminId = GetUserId();
        var adminEmail = GetUserEmail();

        var success = await _adminService.DeleteCategoryAsync(id, adminId, adminEmail);
        if (!success)
        {
            return NotFound(new { message = "Category not found." });
        }

        return NoContent();
    }

    // ==========================================
    // 3. VIEW SYSTEM-WIDE STATISTICS
    // ==========================================
    [HttpGet("stats")]
    public async Task<IActionResult> GetSystemStats()
    {
        var stats = await _adminService.GetSystemStatsAsync();
        return Ok(stats);
    }

    // ==========================================
    // 4. VIEW AUDIT LOGS
    // ==========================================
    [HttpGet("audit-logs")]
    public async Task<IActionResult> GetAuditLogs([FromQuery] int limit = 200)
    {
        var logs = await _adminService.GetAuditLogsAsync(limit);
        return Ok(logs);
    }

    // ==========================================
    // HELPERS
    // ==========================================
    private int GetUserId()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(userId, out var id) ? id : 0;
    }

    private string GetUserEmail()
    {
        return User.FindFirstValue(ClaimTypes.Email) ?? "admin@system";
    }
}
