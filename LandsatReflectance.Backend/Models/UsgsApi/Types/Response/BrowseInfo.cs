namespace LandsatReflectance.Backend.Models.UsgsApi.Types;

public class BrowseInfo
{
    public string Id { get; set; } = string.Empty;
    public bool? BrowseRotationEnabled { get; set; }
    public string BrowseName { get; set; } = string.Empty;
    public string BrowsePath { get; set; } = string.Empty;
    public string OverlayPath { get; set; } = string.Empty;
    public string OverlayType { get; set; } = string.Empty;
    public string ThumbnailPath { get; set; } = string.Empty;
}