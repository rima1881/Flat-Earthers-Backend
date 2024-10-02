namespace LandsatReflectance.Backend.Models;

public class Target
{
    public Guid Guid { get; init; } = Guid.NewGuid();
    
    public int Path { get; set; }
    public int Row { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? MinCloudCover { get; set; } = 0;
    public double? MaxCloudCover { get; set; } = 1;
    
    public TimeSpan NotificationOffset { get; set; } = TimeSpan.Zero;
}