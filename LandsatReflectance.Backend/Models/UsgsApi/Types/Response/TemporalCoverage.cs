using System.Text.Json.Serialization;
using LandsatReflectance.Backend.Utils;

namespace LandsatReflectance.Backend.Models.UsgsApi.Types;

public class TemporalCoverage
{
    [JsonConverter(typeof(CustomDateTimeConverter))]
    public DateTime StartDate { get; set; } = DateTime.MinValue;
    
    [JsonConverter(typeof(CustomDateTimeConverter))]
    public DateTime EndDate { get; set; } = DateTime.MinValue;
}