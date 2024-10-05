using System.Text.Json;
using LandsatReflectance.Backend.Models.UsgsApi;
using LandsatReflectance.Backend.Models.UsgsApi.Endpoints;
using LandsatReflectance.Backend.Utils;

namespace LandsatReflectance.Backend.Tests.SerializationTests;

public class SceneListAddSerializationTests
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
                new UsgsApiResponseConverter<SceneListAddResponse>(),
                new MetadataConverter()
            }
        };
        
        
        string rawJson = File.ReadAllText("Data/Endpoints/SampleResponses/scene-list-add-1.json");
        var obj = JsonSerializer.Deserialize<UsgsApiResponse<SceneListAddResponse>>(rawJson, jsonSerializerOptions);
        
        Assert.Pass(obj?.ToString());
    }
}