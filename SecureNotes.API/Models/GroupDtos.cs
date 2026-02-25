namespace SecureNotes.Api.Models;

public sealed class CreateGroupRequest
{
    public string Name { get; set; } = "Моя група";
}

public sealed class JoinGroupRequest
{
    public string InviteCode { get; set; } = string.Empty;
}

public sealed class GroupResponse
{
    public int Id { get; set; }
    public int OwnerId { get; set; }
    public string InviteCode { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
