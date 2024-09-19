using System.Text.Json;
using LandsatReflectance.Backend.Models.UsgsApi;
using LandsatReflectance.Backend.Models.UsgsApi.Endpoints;
using LandsatReflectance.Backend.Utils;

namespace LandsatReflectance.Backend.Tests;

public class SceneMetadataListSerializationTests
{
    [Test]
    public async Task Test2()
    {
        var jsonSerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            Converters =
            {
                new UsgsApiResponseConverter<SceneMetadataListResponse>(),
                new MetadataConverter()
            }
        };
        
        
        string rawJson = File.ReadAllText("Data/Endpoints/SampleResponses/scene-metadata-list-1.json");
        var obj = JsonSerializer.Deserialize<UsgsApiResponse<SceneMetadataListResponse>>(rawJson, jsonSerializerOptions);
        
        Assert.Pass(obj?.ToString());
    }
}