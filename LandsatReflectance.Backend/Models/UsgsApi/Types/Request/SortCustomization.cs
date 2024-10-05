using System.Text.Json.Serialization;

namespace LandsatReflectance.Backend.Models.UsgsApi.Types.Request;

public class SortCustomization
{
    [JsonPropertyName("field_name")]
    public string FieldName { get; set; } = string.Empty;

    public string Direction { get; set; } = string.Empty;
}
