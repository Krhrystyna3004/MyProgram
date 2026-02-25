using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureNotes.Api.Models;
using SecureNotes.Api.Services;

namespace SecureNotes.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class PinController(DbService db) : ControllerBase
{
    private int CurrentUserId()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(id, out var userId))
            throw new UnauthorizedAccessException("Invalid token.");
        return userId;
    }

    [HttpPost("set")]
    public IActionResult SetPin([FromBody] SetPinRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.NewPin) || req.NewPin.Length < 4)
            return BadRequest("PIN must be at least 4 chars.");

        var userId = CurrentUserId();
        var salt = CryptoService.GenerateSalt();
        var hash = CryptoService.HashWithPBKDF2(req.NewPin, salt);
        db.UpdateUserPin(userId, hash, salt);

        return NoContent();
    }

    [HttpPost("change")]
    public IActionResult ChangePin([FromBody] ChangePinRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.NewPin) || req.NewPin.Length < 4)
            return BadRequest("New PIN must be at least 4 chars.");

        var userId = CurrentUserId();
        var (pinHash, pinSalt) = db.GetPinData(userId);

        if (string.IsNullOrWhiteSpace(pinHash) || string.IsNullOrWhiteSpace(pinSalt))
            return BadRequest("PIN is not set.");

        var oldHash = CryptoService.HashWithPBKDF2(req.OldPin, pinSalt);
        var newSalt = CryptoService.GenerateSalt();
        var newHash = CryptoService.HashWithPBKDF2(req.NewPin, newSalt);

        db.UpdateUserPinWithCheck(userId, oldHash, newHash, newSalt);
        return NoContent();
    }

    [HttpPost("verify")]
    public ActionResult<VerifyPinResponse> Verify([FromBody] VerifyPinRequest req)
    {
        var userId = CurrentUserId();
        var (pinHash, pinSalt) = db.GetPinData(userId);

        if (string.IsNullOrWhiteSpace(pinHash) || string.IsNullOrWhiteSpace(pinSalt))
            return Ok(new VerifyPinResponse { IsValid = false });

        var hash = CryptoService.HashWithPBKDF2(req.Pin, pinSalt);
        return Ok(new VerifyPinResponse { IsValid = hash == pinHash });
    }
}
