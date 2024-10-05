using System.Text.Json.Serialization;
using LandsatReflectance.Backend.Models.UsgsApi.Types;

namespace LandsatReflectance.Backend.Models.UsgsApi;

public class SceneData
{
    [JsonPropertyName("browse")]
    public BrowseInfo[] BrowseInfos { get; set; } = [];
    
    public int CloudCover { get; set; }
    public string EntityId { get; set; } = string.Empty;
    public string DisplayId { get; set; } = string.Empty;
    public string? OrderingId { get; set; } 
    public bool? HasCustomizedMetadata { get; set; }

    public Metadata[] Metadata { get; set; } = [];
    public QueryOptions? Options { get; set; }
    public Selected? Selected { get; set; }
    public SpatialArea? SpatialBounds { get; set; }
    public SpatialArea? SpatialCoverage { get; set; }
    public TemporalCoverage? TemporalCoverage { get; set; }
    public DateTime PublishDate { get; set; } = DateTime.MinValue;
}