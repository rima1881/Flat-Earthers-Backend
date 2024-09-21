using System.Text.Json;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using LandsatReflectance.Backend.Models.UsgsApi;
using LandsatReflectance.Backend.Models.UsgsApi.Endpoints;
using LandsatReflectance.Backend.Utils.SourceGenerators;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace LandsatReflectance.Backend.Tests.Benchmarks;

public class Benchmark 
{
    private HttpClient _client = null!;
    private WebApplicationFactory<Program> _factory = null!;

    private int path;
    private int row;

    [GlobalSetup]
    public void Setup()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient();
    }

    [Benchmark]
    public async Task GetEndpointBenchmark()
    {
        var serviceProvider = _factory.Services;
        var jsonOptions = serviceProvider.GetRequiredService<IOptions<JsonOptions>>();
        var jsonSerializerOptions = jsonOptions.Value.SerializerOptions;
        
        Assert.IsNotNull(jsonSerializerOptions);
        
        // Removes the json context for 'SceneSearchResponse'
        for (int i = jsonSerializerOptions.TypeInfoResolverChain.Count - 1; i >= 0; i--)
            if (jsonSerializerOptions.TypeInfoResolverChain[i] is SceneSearchResponseJsonContext)
                jsonSerializerOptions.TypeInfoResolverChain.RemoveAt(i);
        
        string rawJson = await File.ReadAllTextAsync("Data/Endpoints/SampleResponses/scene-search-2.json");
        _ = JsonSerializer.Deserialize<UsgsApiResponse<SceneSearchResponse>>(rawJson, jsonSerializerOptions);
    }
    
    
    [Benchmark]
    public async Task GetEndpointBenchmarkWithJsonContext()
    {
        var serviceProvider = _factory.Services;
        var jsonOptions = serviceProvider.GetRequiredService<IOptions<JsonOptions>>();
        var jsonSerializerOptions = jsonOptions.Value.SerializerOptions;
        
        Assert.IsNotNull(jsonSerializerOptions);
        
        string rawJson = await File.ReadAllTextAsync("Data/Endpoints/SampleResponses/scene-search-2.json");
        _ = JsonSerializer.Deserialize<UsgsApiResponse<SceneSearchResponse>>(rawJson, jsonSerializerOptions);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}

public class SceneSearchResponseSerializationBenchmarks
{
    [Test]
    public void BenchmarkPredictionEndpoint()
    {
        // Needed cause we can't compile in release mode
        var config = ManualConfig.Create(DefaultConfig.Instance)
            .WithOptions(ConfigOptions.DisableOptimizationsValidator); 
        
        var summary = BenchmarkRunner.Run<Benchmark>(config);
    }
}