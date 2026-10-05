using System.Security.Claims;
using FinanceManager.API.DTOs;
using FinanceManager.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManager.API.Controllers;

[ApiController]
[Route("api/User")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;

    public AuthController(IAuthService authService, IUserService userService)
    {
        _authService = authService;
        _userService = userService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(UserCreateDto dto)
    {
        var result = await _userService.CreateUserAsync(dto);
        return CreatedAtAction(nameof(Register), new { id = result.Id }, result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginDto dto)
    {
        var result = await _authService.LoginAsync(dto);

        if (result == null)
        {
            return Unauthorized(new
            {
                message = "Invalid email or password."
            });
        }

        return Ok(result);
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken(RefreshTokenRequestDto dto)
    {
        var result = await _authService.RefreshTokenAsync(dto);

        if (result == null)
        {
            return Unauthorized(new
            {
                message = "Invalid or expired refresh token."
            });
        }

        return Ok(result);
    }

    [Authorize]
    [HttpPost("revoke-token")]
    public async Task<IActionResult> RevokeToken()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        var success = await _authService.RevokeTokenAsync(userId);
        if (!success)
        {
            return NotFound(new { message = "User not found." });
        }

        return Ok(new { message = "Refresh token revoked successfully." });
    }
}