using LandsatReflectance.Backend.Models.UsgsApi;
using LandsatReflectance.Backend.Models.UsgsApi.Endpoints;
using LandsatReflectance.Backend.Models.UsgsApi.Types.Request;

namespace LandsatReflectance.Backend.Services;

public static class UsgsDateTimePredictionService
{
    public record SceneDateInfo
    {
        public required DateTime PublishDate { get; init; }
        public required DateTime AcquisitionStartDateTime { get; init; }
        public required DateTime AcquisitionEndDateTime { get; init; }
    }

    public record PredictionResults 
    {
        public required DateTime PredictedPublishDate { get; init; }
        public required TimeSpan AverageTimeSpanBetweenPublishDates { get; init; }
        public required double PredictedPublishDateConfidence { get; init; }
        
        public required DateTime PredictedAcquisitionDate { get; init; }
        public required TimeSpan AverageTimeSpanBetweenAcquisitionDates { get; init; }
        public required double PredictedAcquisitionDateConfidence { get; init; }
        
        public required int PredictedSatellite { get; init; }
    }

    public static async Task<PredictionResults> Predict(UsgsApiService usgsApiService, int path, int row, int numDataEntries = 10)
    {
        var (satellite8DateInfos, satellite9DateInfos) = await GetDateInfos(usgsApiService, path, row, numDataEntries);
        return PredictCore(satellite8DateInfos, satellite9DateInfos);
    }

    internal static PredictionResults PredictCore(SceneDateInfo[] sat8SceneDateInfo, SceneDateInfo[] sat9SceneDateInfo)
    {
        var sat8StartDates = GetStartDates(sat8SceneDateInfo);
        var sat9StartDates = GetStartDates(sat9SceneDateInfo);

        var sat8PublishDates = GetPublishDates(sat8SceneDateInfo);
        var sat9PublishDates = GetPublishDates(sat9SceneDateInfo);
        
        var (predictedStartDate, averageTimespanBetweenStartTimes, predictedStartDateConfidence) =
            CalculatePredicted(sat8StartDates, sat9StartDates);
        
        var (predictedPublishDate, averageTimespanBetweenPublishDates, predictedPublishDateConfidence) =
            CalculatePredicted(sat8PublishDates, sat9PublishDates);

        var predictedSatellite = sat8StartDates[0] > sat9StartDates[0] ? 9 : 8;

        return new PredictionResults
        {
            PredictedPublishDate = predictedPublishDate,
            AverageTimeSpanBetweenPublishDates = averageTimespanBetweenPublishDates,
            PredictedPublishDateConfidence = predictedPublishDateConfidence,
            PredictedAcquisitionDate = predictedStartDate,
            AverageTimeSpanBetweenAcquisitionDates = averageTimespanBetweenStartTimes,
            PredictedAcquisitionDateConfidence = predictedStartDateConfidence,
            PredictedSatellite = predictedSatellite
        };
    }
    
    
    internal static DateTime[] GetStartDates(SceneDateInfo[] sceneDateInfos) => 
        sceneDateInfos.Select(obj => obj.AcquisitionStartDateTime).ToArray();
    
    internal static DateTime[] GetPublishDates(SceneDateInfo[] sceneDateInfos) => 
        sceneDateInfos.Select(obj => obj.PublishDate).ToArray();
    
    
    internal static async Task<(SceneDateInfo[] satellite8DateInfos, SceneDateInfo[] satellite9DateInfos)> GetDateInfos(
        UsgsApiService usgsApiService,
        int path,
        int row,
        int numDataEntries)
    {
        var requests = GetRequests(path, row, numDataEntries);
        var satellite8ResponseTask = usgsApiService.QuerySceneSearch(requests.satellite8DataRequest);
        var satellite9ResponseTask = usgsApiService.QuerySceneSearch(requests.satellite9DataRequest);
        var results = await Task.WhenAll(satellite8ResponseTask, satellite9ResponseTask);

        var satellite8Response = results[0];
        var satellite9Response = results[1];

        if (satellite8Response is null)
            throw new Exception("Scene search response for filtering satellite 8 data was null.");
        
        if (satellite9Response is null)
            throw new Exception("Scene search response for filtering satellite 9 data was null.");

        return (ReduceSceneSearchResponse(satellite8Response), ReduceSceneSearchResponse(satellite9Response));
    }

    private static SceneDateInfo[] ReduceSceneSearchResponse(UsgsApiResponse<SceneSearchResponse> response)
    {
        if (response.Data is null)
            throw new ArgumentException("The response cannot be null");

        return response.Data.ReturnedSceneData
            .Select(ToSceneDateInfo)
            .ToArray();
        
        SceneDateInfo ToSceneDateInfo(SceneData sceneData)
        {
            var publishDate = sceneData.PublishDate;

            DateTime? acquisitionStartDateTime = null;
            DateTime? acquisitionEndDateTime = null;

            foreach (var metadata in sceneData.Metadata)
            {
                if (string.Equals(metadata.Id, "5e83d150f3ba8369"))
                    acquisitionStartDateTime = DateTime.Parse(metadata.Value);
                
                if (string.Equals(metadata.Id, "5e83d1506939e64b"))
                    acquisitionEndDateTime = DateTime.Parse(metadata.Value);
            }
            
            if (acquisitionStartDateTime is null)
                throw new ArgumentException($"Could not find \"Start Time\" metadata for scene entity id \"{sceneData.EntityId}\"");
                    
            if (acquisitionEndDateTime is null)
                throw new ArgumentException($"Could not find \"End Time\" metadata for scene entity id \"{sceneData.EntityId}\"");

            return new SceneDateInfo
            {
                PublishDate = publishDate,
                AcquisitionStartDateTime = acquisitionStartDateTime.Value,
                AcquisitionEndDateTime = acquisitionEndDateTime.Value,
            };
        }
    }

    private static (SceneSearchRequest satellite8DataRequest, SceneSearchRequest satellite9DataRequest) GetRequests(
        int path, 
        int row, 
        int numDataEntries)
    {
        var pathFilter = new MetadataFilterValue
        {
            FilterId = "5e83d14fb9436d88",
            Value = path.ToString(),
            Operand = MetadataFilterValue.MetadataValueOperand.Equals
        };
        var rowFilter = new MetadataFilterValue
        {
            FilterId = "5e83d14ff1eda1b8",
            Value = row.ToString(),
            Operand = MetadataFilterValue.MetadataValueOperand.Equals
        };
        var getSatelliteFilter = (int satelliteNumber) => new MetadataFilterValue
        {
            FilterId = "61af9273566bb9a8",
            Value = satelliteNumber.ToString(),
            Operand = MetadataFilterValue.MetadataValueOperand.Equals
        };
        
        
        var satellite8MetadataFilter = new MetadataFilterAnd 
        {
            ChildFilters = [ pathFilter, rowFilter, getSatelliteFilter(8) ] 
        };
        var satellite9MetadataFilter = new MetadataFilterAnd 
        {
            ChildFilters = [ pathFilter, rowFilter, getSatelliteFilter(9) ] 
        };
        
        
        var sceneSearchRequestSatellite8 = new SceneSearchRequest
        {
            DatasetName = "landsat_ot_c2_l2",
            MaxResults = numDataEntries,
            UseCustomization = false,
            SceneFilter = new SceneFilter
            {
                MetadataFilter = satellite8MetadataFilter
            },
        };
        var sceneSearchRequestSatellite9 = new SceneSearchRequest
        {
            DatasetName = "landsat_ot_c2_l2",
            MaxResults = numDataEntries,
            UseCustomization = false,
            SceneFilter = new SceneFilter
            {
                MetadataFilter = satellite9MetadataFilter 
            },
        };

        return (sceneSearchRequestSatellite8, sceneSearchRequestSatellite9);
    }
    
    internal static (DateTime predictedTime, TimeSpan averageTimeSpan, double confidenceLevel) CalculatePredicted(DateTime[] satellite8Dates, DateTime[] satellite9Dates)
    {
        var datesToProcess = satellite8Dates[0] > satellite9Dates[0] 
            ? satellite9Dates 
            : satellite8Dates;
        
        /*
        var otherDates = satellite8Dates[0] > satellite9Dates[0] 
            ? satellite8Dates 
            : satellite9Dates;
         */

        var timespans = new TimeSpan[datesToProcess.Length - 1];
        for (int i = 0; i < datesToProcess.Length - 1; i++)
        {
            timespans[i] = datesToProcess[i] - datesToProcess[i + 1];
        }

        var sum = timespans.Aggregate(TimeSpan.Zero, (acc, timeSpan) => timeSpan + acc);
        var avgTicks = sum.Ticks / timespans.Length;
        var avgTimespan = TimeSpan.FromTicks(avgTicks);

        var predictedTime = datesToProcess[0] + avgTimespan;

        return (predictedTime, avgTimespan, CalculateConfidenceLevel(timespans));


        double CalculateConfidenceLevel(TimeSpan[] ts)
        {
            // 1. Calculate variance
            int n = ts.Length;
            var asTicks = ts.Select(timeSpan => timeSpan.Ticks).ToArray();
            double mean = asTicks.Average();
            double sumOfSquares = asTicks.Select(ticks => Math.Pow(ticks - mean, 2)).Sum();
            double variance = sumOfSquares / n;
            double normalizedVariance = Math.Log10(1 + variance);

            // 2. Normalize to a confidence level
            const double maxVariance = 50;
            double clampedVariance = Math.Min(normalizedVariance, maxVariance);

            return 1 - Math.Pow(clampedVariance / (0.6 * maxVariance), 5);
            // return Math.Pow(Math.E, -Math.Pow(clampedVariance / maxVariance, 2));
            // return 1 - (newVariance / maxVariance);
        }
    }
}