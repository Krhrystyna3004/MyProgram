using System.Security.Cryptography;

namespace SecureNotes.Api.Services;

public static class CryptoService
{
    public static string GenerateSalt(int bytes = 16)
    {
        var salt = new byte[bytes];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(salt);
        return Convert.ToBase64String(salt);
    }

    public static string HashWithPBKDF2(string value, string saltBase64, int iterations = 100_000, int bytes = 32)
    {
        var salt = Convert.FromBase64String(saltBase64);
        using var pbkdf2 = new Rfc2898DeriveBytes(value, salt, iterations, HashAlgorithmName.SHA256);
        return Convert.ToBase64String(pbkdf2.GetBytes(bytes));
    }
}