namespace SecureNotes.Api.Models;

public sealed class UserEntity
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string PasswordSalt { get; set; } = string.Empty;
    public string? PinHash { get; set; }
    public string? PinSalt { get; set; }
}

public sealed class NoteEntity
{
    public int Id { get; set; }
    public int OwnerId { get; set; }
    public int? GroupId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Type { get; set; } = "note";
    public string Color { get; set; } = "#FFFFFF";
    public string Tags { get; set; } = string.Empty;
    public string? IvBase64 { get; set; }
    public string Attachments { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public sealed class GroupEntity
{
    public int Id { get; set; }
    public int OwnerId { get; set; }
    public string InviteCode { get; set; } = string.Empty;
    public string Name { get; set; } = "Моя група";
    public DateTime CreatedAt { get; set; }
}
