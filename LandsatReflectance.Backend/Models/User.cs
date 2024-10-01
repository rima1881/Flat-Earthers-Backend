namespace LandsatReflectance.Backend.Models;

public class User
{
    public Guid Guid { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public Target[] Targets { get; set; } = [];
}