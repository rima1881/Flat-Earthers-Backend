namespace LandsatReflectance.Backend.Models;

public class User
{
    public string Email { get; set; } = string.Empty;
    public SelectedRegion[] SelectedRegions { get; set; } = [];
}

public class SelectedRegion
{
    public string Path { get; set; } = string.Empty;
    public string Row { get; set; } = string.Empty;
    public TimeSpan NotificationOffset { get; set; } = TimeSpan.Zero;
}