using System.Text.Json.Serialization;
using LandsatReflectance.Backend.Models.UsgsApi.Types;

namespace LandsatReflectance.Backend.Models.UsgsApi.Endpoints;

public class SceneSearchRequest
{
    
}

public class SceneSearchResponse : IUsgsApiResponseData
{
    [JsonPropertyName("browse")]
    public BrowseInfo[] BrowseInfos { get; set; } = [];
    
    public int CloudCover { get; set; }
    public string EntityId { get; set; } = string.Empty;
    public string DisplayId { get; set; } = string.Empty;
    public string? OrderingId { get; set; }

    public Metadata[] Metadata { get; set; } = [];
    
    public bool HasCustomizedMetadata { get; set; }

    public Options? Options { get; set; }
    public Selected? Selected { get; set; }
    public SpatialArea? SpatialBounds { get; set; }
    public SpatialArea? SpatialCoverage { get; set; }
    public TemporalCoverage? TemporalCoverage { get; set; }
    
    public DateTime DateTime { get; set; } = DateTime.MinValue;
}
