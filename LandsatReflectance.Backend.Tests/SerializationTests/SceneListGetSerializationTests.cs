using System.Text.Json;
using LandsatReflectance.Backend.Models.UsgsApi;
using LandsatReflectance.Backend.Models.UsgsApi.Endpoints;
using LandsatReflectance.Backend.Utils;

namespace LandsatReflectance.Backend.Tests.SerializationTests;

public class SceneListGetSerializationTests
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
                new UsgsApiResponseConverter<SceneListGetResponse>(),
                new MetadataConverter()
            }
        };
        
        
        string rawJson = File.ReadAllText("Data/Endpoints/SampleResponses/scene-list-get-1.json");
        var obj = JsonSerializer.Deserialize<UsgsApiResponse<SceneListGetResponse>>(rawJson, jsonSerializerOptions);
        
        Assert.Pass(obj?.ToString());
    }
}