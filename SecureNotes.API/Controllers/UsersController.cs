using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecureNotes.Api.Models;
using SecureNotes.Api.Services;

namespace SecureNotes.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public sealed class UsersController(DbService db) : ControllerBase
{
    private int CurrentUserId()
    {
        var id = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(id, out var userId))
            throw new UnauthorizedAccessException("Invalid token.");
        return userId;
    }

    [HttpPut("theme")]
    public IActionResult UpdateTheme([FromBody] UpdateThemeRequest req)
    {
        var userId = CurrentUserId();
        if (string.IsNullOrWhiteSpace(req.Theme))
            return BadRequest("Theme is required.");

        db.UpdateUserTheme(userId, req.Theme.Trim());
        return NoContent();
    }

    [HttpPut("password")]
    public IActionResult ChangePassword([FromBody] ChangePasswordRequest req)
    {
        var userId = CurrentUserId();

        if (string.IsNullOrWhiteSpace(req.CurrentPassword) || string.IsNullOrWhiteSpace(req.NewPassword))
            return BadRequest("CurrentPassword and NewPassword are required.");

        if (req.NewPassword.Length < 6)
            return BadRequest("New password must be at least 6 chars.");

        var user = db.GetUserByUsername(User.Identity?.Name ?? string.Empty);
        if (user is null)
            return Unauthorized("User not found.");

        var currentHash = CryptoService.HashWithPBKDF2(req.CurrentPassword, user.PasswordSalt);
        if (!string.Equals(currentHash, user.PasswordHash, StringComparison.Ordinal))
            return Unauthorized("Current password is incorrect.");

        var newSalt = CryptoService.GenerateSalt();
        var newHash = CryptoService.HashWithPBKDF2(req.NewPassword, newSalt);

        db.UpdateUserPassword(userId, newHash, newSalt);
        return NoContent();
    }

    [HttpDelete("me")]
    public IActionResult DeleteMyAccount()
    {
        var userId = CurrentUserId();
        db.DeleteUser(userId);
        return NoContent();
    }
}
