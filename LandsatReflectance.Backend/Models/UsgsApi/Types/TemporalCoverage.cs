namespace LandsatReflectance.Backend.Models.UsgsApi.Types;

public class TemporalCoverage
{
    public DateTime StartDate { get; set; } = DateTime.MinValue;
    public DateTime EndDate { get; set; } = DateTime.MinValue;
}