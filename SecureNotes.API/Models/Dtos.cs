namespace SecureNotes.Api.Models;

public sealed class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public sealed class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public sealed class AuthResponse
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
}

public sealed class UpsertNoteRequest
{
    public int? GroupId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Type { get; set; } = "note";
    public string Color { get; set; } = "#FFFFFF";
    public string Tags { get; set; } = string.Empty;
    public string? IvBase64 { get; set; }
    public string Attachments { get; set; } = string.Empty;
}