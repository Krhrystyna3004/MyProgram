namespace SecureNotes.Api.Models;

public sealed class UpdateThemeRequest
{
    public string Theme { get; set; } = "Light";
}

public sealed class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public sealed class SetPinRequest
{
    public string NewPin { get; set; } = string.Empty;
}

public sealed class ChangePinRequest
{
    public string OldPin { get; set; } = string.Empty;
    public string NewPin { get; set; } = string.Empty;
}

public sealed class VerifyPinRequest
{
    public string Pin { get; set; } = string.Empty;
}

public sealed class VerifyPinResponse
{
    public bool IsValid { get; set; }
}
