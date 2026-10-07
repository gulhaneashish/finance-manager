using System.Security.Claims;
using System.Text.Json;
using FinanceManager.API.Models;
using FinanceManager.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinanceManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QrCodeController : ControllerBase
{
    private readonly IAccountRepository _accountRepository;
    private readonly IUserRepository _userRepository;

    public QrCodeController(
        IAccountRepository accountRepository,
        IUserRepository userRepository)
    {
        _accountRepository = accountRepository;
        _userRepository = userRepository;
    }

    private int GetCurrentUserId()
    {
        var claim = User.FindFirst(ClaimTypes.NameIdentifier);
        return claim != null && int.TryParse(claim.Value, out var id) ? id : 0;
    }

    [HttpGet("account/{accountId}")]
    public async Task<IActionResult> GetAccountQrPayload(int accountId, [FromQuery] decimal? amount = null, [FromQuery] string? note = null)
    {
        var userId = GetCurrentUserId();
        var account = await _accountRepository.FirstOrDefaultAsync(a => a.Id == accountId && a.UserId == userId && a.IsActive);
        if (account == null)
        {
            return NotFound(new { message = "Account not found or inactive." });
        }

        var user = await _userRepository.GetByIdAsync(userId);

        var payload = new
        {
            scheme = "finman",
            action = "pay_account",
            version = "1.0",
            accountId = account.Id,
            accountName = account.Name,
            accountType = account.AccountType,
            userId = user?.Id ?? userId,
            userName = user?.Name ?? "User",
            amount = amount.HasValue && amount.Value > 0 ? amount.Value : (decimal?)null,
            note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
            createdAt = DateTime.UtcNow
        };

        return Ok(payload);
    }

    public record VerifyQrRequest(string QrData);

    [HttpPost("verify")]
    public async Task<IActionResult> VerifyQrPayload([FromBody] VerifyQrRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.QrData))
        {
            return BadRequest(new { message = "QR data is empty." });
        }

        try
        {
            using var doc = JsonDocument.Parse(request.QrData);
            var root = doc.RootElement;

            if (!root.TryGetProperty("accountId", out var accountIdElem))
            {
                return BadRequest(new { message = "Invalid QR format: missing accountId." });
            }

            var accountId = accountIdElem.GetInt32();
            var account = await _accountRepository.GetByIdAsync(accountId);

            if (account == null || !account.IsActive)
            {
                return BadRequest(new { message = "Destination account does not exist or is inactive." });
            }

            var accountOwner = await _userRepository.GetByIdAsync(account.UserId);

            decimal? amount = null;
            if (root.TryGetProperty("amount", out var amountElem) && amountElem.ValueKind == JsonValueKind.Number)
            {
                amount = amountElem.GetDecimal();
            }

            string? note = null;
            if (root.TryGetProperty("note", out var noteElem) && noteElem.ValueKind == JsonValueKind.String)
            {
                note = noteElem.GetString();
            }

            return Ok(new
            {
                valid = true,
                accountId = account.Id,
                accountName = account.Name,
                accountType = account.AccountType,
                ownerName = accountOwner?.Name ?? "User",
                suggestedAmount = amount,
                note = note
            });
        }
        catch (JsonException)
        {
            return BadRequest(new { message = "Invalid QR code payload format. Expected JSON payment payload." });
        }
    }
}
