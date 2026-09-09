using FinanceManager.API.DTOs;
using FinanceManager.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManager.API.Controllers;

[ApiController]
[Route("api/User")]
public class AuthController : ControllerBase
{
    private readonly AuthService _authService;

    public AuthController(AuthService authService)
    {
        _authService = authService;
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
}