namespace LandsatReflectance.Backend.Models;

public class User
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public SelectedRegion[] SelectedRegions { get; set; } = [];
}

public class SelectedRegion
{
    public int Path { get; set; }
    public int Row { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public TimeSpan NotificationOffset { get; set; } = TimeSpan.Zero;
}