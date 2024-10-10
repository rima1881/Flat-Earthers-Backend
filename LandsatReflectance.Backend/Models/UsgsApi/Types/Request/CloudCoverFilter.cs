namespace LandsatReflectance.Backend.Models.UsgsApi.Types.Request;

public class CloudCoverFilter
{
    public int Min { get; set; } = 0;
    public int Max { get; set; } = 100;
    public bool IncludeUnknown { get; set; } = true;
}