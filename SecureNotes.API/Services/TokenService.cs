using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace SecureNotes.Api.Services;

public sealed class TokenService(IConfiguration config)
{
    public string CreateToken(int userId, string username)
    {
        var issuer = config["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer missing");
        var audience = config["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience missing");
        var key = config["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key missing");
        var expiresMinutes = int.TryParse(config["Jwt:ExpiresMinutes"], out var minutes) ? minutes : 60;

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, username)
        };

        var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}