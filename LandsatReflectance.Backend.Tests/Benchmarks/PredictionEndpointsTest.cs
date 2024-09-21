using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace LandsatReflectance.Backend.Tests.Benchmarks;

public class ApiBenchmark
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

        path = 14;
        row = 28;
    }

    [Benchmark]
    [IterationCount(15)]
    [WarmupCount(0)]
    public async Task GetEndpointBenchmark()
    {
        var response = await _client.GetAsync("/Prediction?path=14&row=28");
        response.EnsureSuccessStatusCode();
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}

public class PredictionEndpointsTest
{
    [Test]
    public void BenchmarkPredictionEndpoint()
    {
        var config = ManualConfig.Create(DefaultConfig.Instance)
            .WithOptions(ConfigOptions.DisableOptimizationsValidator); 
        
        var summary = BenchmarkRunner.Run<ApiBenchmark>(config);
    }
}