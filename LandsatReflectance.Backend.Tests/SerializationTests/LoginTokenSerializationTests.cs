using System.Text.Json;
using LandsatReflectance.Backend.Models.UsgsApi;
using LandsatReflectance.Backend.Models.UsgsApi.Endpoints;
using LandsatReflectance.Backend.Services;
using LandsatReflectance.Backend.Utils;

namespace LandsatReflectance.Backend.Tests;

public class LoginTokenSerializationTests 
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
                new UsgsApiResponseConverter<LoginTokenResponse>(),
                new MetadataConverter()
            }
        };
        
        
        string rawJson = File.ReadAllText("Data/Endpoints/SampleResponses/login-token-1.json");
        var response = JsonSerializer.Deserialize<UsgsApiResponse<LoginTokenResponse>>(rawJson, jsonSerializerOptions);
        
        Assert.Pass();
    }
    
    
    [Test]
    public void Test2()
    {
        var jsonSerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            Converters =
            {
                new UsgsApiResponseConverter<LoginTokenResponse>(),
                new MetadataConverter()
            }
        };


        var service = new UsgsApiService();
        
        // var response = JsonSerializer.Deserialize<UsgsApiResponse<LoginTokenResponse>>(rawJson, jsonSerializerOptions);
        
        Assert.Pass();
    }
}
