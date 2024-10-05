using System.Text.Json;
using LandsatReflectance.Backend.Services;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LandsatReflectance.Backend.Tests.Predictions;

internal class PredictionResultsComparison 
{
    public required UsgsDateTimePredictionService.PredictionResults PredictionResults { get; init; }
    
    public required int ActualSatellite { get; init; }
    public required UsgsDateTimePredictionService.SceneDateInfo ActualSceneDateInfo { get; init; }
    
    public required double AcquisitionDateMarginOfError { get; init; }
    public required double PublishDateMarginOfError { get; init; }
}

public class PredictionTests
{
    /// The number of scenes to collect when doing the prediction.
    private const int NumDataEntries = 10;
    
    private WebApplicationFactory<Program> m_factory = null!;

    private IServiceScope m_serviceScope = null!;
    private UsgsApiService m_usgsApiService = null!;
    private JsonSerializerOptions m_jsonSerializerOptions = null!;
    
    
    [SetUp]
    public void Setup()
    {
        // Store factory to access web-app services and config
        m_factory = new WebApplicationFactory<Program>();

        m_serviceScope = m_factory.Services.CreateScope();
        m_usgsApiService = m_serviceScope.ServiceProvider.GetRequiredService<UsgsApiService>();
        m_jsonSerializerOptions = m_factory.Services.GetRequiredService<IOptions<JsonOptions>>().Value.SerializerOptions;
    }


    [Test]
    public async Task PredictionTest()
    {
        var predictionResultsComparison = await PerformPredictionAndComparison(25, 45);
        Assert.Pass(JsonSerializer.Serialize(predictionResultsComparison, m_jsonSerializerOptions));
    }

    private async Task<PredictionResultsComparison> PerformPredictionAndComparison(int path, int row)
    {
        var (sat8DateInfos, sat9DateInfos) = await UsgsDateTimePredictionService.GetDateInfos(m_usgsApiService, path, row, NumDataEntries);

        Func<UsgsDateTimePredictionService.SceneDateInfo, DateTime> getPropertyToCompare =
            sceneDateInfo => sceneDateInfo.AcquisitionStartDateTime;

        // We're going to remove the most recent date from the array of values.
        // Then we calculate the prediction from those remaining values, and compare it to the one we removed.
        int actualSatellite;
        UsgsDateTimePredictionService.SceneDateInfo actualDateInfo;

        if (getPropertyToCompare(sat8DateInfos[0]) > getPropertyToCompare(sat9DateInfos[0]))
        {
            actualSatellite = 8;
            actualDateInfo = sat8DateInfos[0];
            sat8DateInfos = sat8DateInfos.Skip(1).ToArray();
        }
        else
        {
            actualSatellite = 9;
            actualDateInfo = sat9DateInfos[0];
            sat9DateInfos = sat9DateInfos.Skip(1).ToArray();
        }

        var predictionResults = UsgsDateTimePredictionService.PredictCore(sat8DateInfos, sat9DateInfos);

        return new PredictionResultsComparison
        {
            PredictionResults = predictionResults,
            ActualSatellite = actualSatellite,
            ActualSceneDateInfo = actualDateInfo,
            AcquisitionDateMarginOfError = CalculateMarginOfError(predictionResults.PredictedAcquisitionDate, actualDateInfo.AcquisitionStartDateTime),
            PublishDateMarginOfError = CalculateMarginOfError(predictionResults.PredictedPublishDate, actualDateInfo.PublishDate)
        };
    }

    private static double CalculateMarginOfError(DateTime predictedValue, DateTime actualValue)
    {
        double hourDifference = Math.Abs((actualValue - predictedValue).TotalHours);
        double k = 0.1;
        return 100 / (1 + Math.Exp(-k * hourDifference));
    }
    
    private static (int, int)[] GeneratePathAndRowCombinations()
    {
        int[] validPaths = [ 14 ];
        int[] validRows = [ 28 ];
        
        return (from path in validPaths from row in validRows select (path, row)).ToArray();
    }

    [TearDown]
    public void Teardown()
    {
        m_serviceScope.Dispose();
        m_factory.Dispose();
    }
}