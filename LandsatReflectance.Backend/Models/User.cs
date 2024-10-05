namespace LandsatReflectance.Backend.Models;

public class User
{
    public Guid Guid { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsEmailEnabled { get; set; } = true;

    public string ToLogString() => $"User: ({Guid}, {Email}, {PasswordHash}, {IsEmailEnabled})";
}