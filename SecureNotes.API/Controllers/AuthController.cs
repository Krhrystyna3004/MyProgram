using Microsoft.AspNetCore.Mvc;
using SecureNotes.Api.Models;
using SecureNotes.Api.Services;

namespace SecureNotes.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController(DbService db, TokenService tokens) : ControllerBase
{
    [HttpPost("register")]
    public ActionResult<AuthResponse> Register([FromBody] RegisterRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
            return BadRequest("Username and password are required.");

        if (db.GetUserByUsername(req.Username) is not null)
            return Conflict("User already exists.");

        var salt = CryptoService.GenerateSalt();
        var hash = CryptoService.HashWithPBKDF2(req.Password, salt);
        var userId = db.CreateUser(req.Username.Trim(), hash, salt);
        var token = tokens.CreateToken(userId, req.Username.Trim());

        return Ok(new AuthResponse { UserId = userId, Username = req.Username.Trim(), AccessToken = token });
    }

    [HttpPost("login")]
    public ActionResult<AuthResponse> Login([FromBody] LoginRequest req)
    {
        var user = db.GetUserByUsername(req.Username.Trim());
        if (user is null)
            return Unauthorized("Invalid credentials.");

        var calc = CryptoService.HashWithPBKDF2(req.Password, user.PasswordSalt);
        if (!string.Equals(calc, user.PasswordHash, StringComparison.Ordinal))
            return Unauthorized("Invalid credentials.");

        var token = tokens.CreateToken(user.Id, user.Username);
        return Ok(new AuthResponse { UserId = user.Id, Username = user.Username, AccessToken = token });
    }
}