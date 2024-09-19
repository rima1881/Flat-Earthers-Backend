using System.Text.Json;
using System.Text.Json.Serialization;
using LandsatReflectance.Backend.Models.UsgsApi;

namespace LandsatReflectance.Backend.Utils;

public class SceneDataSimplifiedConverter : JsonConverter<SceneData>
{
    public override SceneData? Read(ref Utf8JsonReader reader, Type _, JsonSerializerOptions options)
    {
        using var jsonDocument = JsonDocument.ParseValue(ref reader);
        string rawJson = jsonDocument.RootElement.GetRawText();
        return JsonSerializer.Deserialize<SceneData>(rawJson, options);
    }

    public override void Write(Utf8JsonWriter writer, SceneData sceneData, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        
        if (sceneData.BrowseInfos.Length > 0)
            writer.WriteString("browse", sceneData.BrowseInfos[0].BrowsePath);
        else 
            writer.WriteNull("browse");
        
        writer.WriteString("entityId", sceneData.EntityId);
        writer.WriteNumber("cloudCover", sceneData.CloudCover);
        writer.WriteString("publishDate", sceneData.PublishDate);
        
        writer.WriteEndObject();
    }
}