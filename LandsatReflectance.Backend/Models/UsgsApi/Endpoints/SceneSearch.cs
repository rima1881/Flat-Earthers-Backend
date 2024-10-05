using System.Text.Json.Serialization;
using LandsatReflectance.Backend.Models.UsgsApi.Types;
using LandsatReflectance.Backend.Models.UsgsApi.Types.Request;

namespace LandsatReflectance.Backend.Models.UsgsApi.Endpoints;

[Serializable]
public class SceneSearchRequest
{
    public string DatasetName { get; set; } = string.Empty;
    public int MaxResults { get; set; } = 100;
    public int StartingNumber { get; set; }
    
    public string MetadataType { get; set; } = "full";
    public string? SortField { get; set; }
    public string? SortDirection { get; set; }
    public string? CompareListName { get; set; }
    public string? BulkListName { get; set; }
    public string? OrderListName { get; set; }
    public string? ExcludeListName { get; set; }

    public bool UseCustomization { get; set; } = false;
    public bool IncludeNullMetadataValues { get; set; } = false;
    
    public SortCustomization? SortCustomization { get; set; } = new();
    public SceneFilter? SceneFilter { get; set; } = new();
}

[Serializable]
public class SceneSearchResponse : IUsgsApiResponseData
{
    [JsonPropertyName("results")]
    public SceneData[] ReturnedSceneData { get; set; } = [];
    
    public int RecordsReturned { get; set; }
    public int TotalHits { get; set; }
    public string TotalHitsAccuracy { get; set; } = string.Empty;
    public bool IsCustomized { get; set; }
    public int NumExcluded { get; set; }
    public int StartingNumber { get; set; }
    public int NextRecord { get; set; }
}