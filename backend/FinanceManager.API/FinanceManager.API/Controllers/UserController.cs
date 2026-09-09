using FinanceManager.API.DTOs;
using FinanceManager.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FinanceManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _userService.GetUsersAsync();

        return Ok(users);
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser(UserCreateDto dto)
    {
        var user = await _userService.CreateUserAsync(dto);

        return Ok(user);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized();
        }

        var user = await _userService.GetUserByIdAsync(
            int.Parse(userId));

        if (user == null)
        {
            return NotFound();
        }

        return Ok(new
        {
            userId = user.Id,
            name = user.Name,
            email = user.Email
        });
    }
}