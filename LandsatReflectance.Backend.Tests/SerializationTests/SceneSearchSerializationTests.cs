using System.Text.Json;
using LandsatReflectance.Backend.Models.UsgsApi;
using LandsatReflectance.Backend.Models.UsgsApi.Endpoints;
using LandsatReflectance.Backend.Utils;

namespace LandsatReflectance.Backend.Tests;

public class SceneSearchSerializationTests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        var jsonSerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            Converters =
            {
                new UsgsApiResponseConverter<SceneSearchResponse>(),
                new MetadataConverter()
            }
        };
        
        
        string rawJson = File.ReadAllText("Data/Endpoints/SampleResponses/scene-search-1.json");
        var response = JsonSerializer.Deserialize<UsgsApiResponse<SceneSearchResponse>>(rawJson, jsonSerializerOptions);
        
        Assert.Pass();
    }
}
