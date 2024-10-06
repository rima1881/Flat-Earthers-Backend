using System.Text.Json;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Http.Json;
using LandsatReflectance.Backend.Models;
using LandsatReflectance.Backend.Models.UsgsApi;
using LandsatReflectance.Backend.Models.UsgsApi.Endpoints;
using LandsatReflectance.Backend.Models.UsgsApi.Types;
using LandsatReflectance.Backend.Models.UsgsApi.Types.Request;
using LandsatReflectance.Backend.Services.NotificationSender;
using PredictionService = LandsatReflectance.Backend.Services.UsgsDateTimePredictionService;
using PredictionResults = LandsatReflectance.Backend.Services.UsgsDateTimePredictionService.PredictionResults;


namespace LandsatReflectance.Backend.Services.BackgroundServices;


public class NotificationService : BackgroundService
{
    private readonly TimeSpan m_checkInterval = TimeSpan.FromMinutes(15);
    private readonly TimeSpan m_maxNotificationOffset = TimeSpan.FromDays(1);

    private readonly IServiceScopeFactory m_serviceScopeFactory;
    private readonly ILogger<NotificationService> m_logger;
    private readonly JsonSerializerOptions m_jsonSerializerOptions;

    
    public NotificationService(IServiceScopeFactory serviceScopeFactory, ILogger<NotificationService> logger, IOptions<JsonOptions> jsonOptions)
    {
        m_serviceScopeFactory = serviceScopeFactory;
        m_logger = logger;
        m_jsonSerializerOptions = jsonOptions.Value.SerializerOptions;  
    }
    
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await PerformChecks(stoppingToken);
            await Task.Delay(m_checkInterval, stoppingToken);
        }
    }

    private async Task PerformChecks(CancellationToken stoppingToken)
    {
        using var serviceScope = m_serviceScopeFactory.CreateScope();
        var usgsApiService = serviceScope.ServiceProvider.GetRequiredService<UsgsApiService>();
        var usersService = serviceScope.ServiceProvider.GetRequiredService<IUserService>();
        var targetsService = serviceScope.ServiceProvider.GetRequiredService<ITargetService>();
        var userTargetNotificationService = serviceScope.ServiceProvider.GetRequiredService<DbUserTargetNotificationService>();
        var notificationSenderServices = serviceScope.ServiceProvider.GetServices<INotificationSenderService>().ToList();

        foreach ((int _, int _) in targetsService.GetAllRegisteredPathAndRows())
        {
            int path = 13;
            int row = 28;
            
            if (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            
            m_logger.LogInformation($"Examining path & row: {path}, {row}.");

            
            const int numResults = 10;
            var sceneSearchRequest = CreateSceneSearchRequest(path, row, numResults);

            var sceneSearchResponse = await usgsApiService.QuerySceneSearch(sceneSearchRequest);
            if (sceneSearchResponse.Data is null)
            {
                m_logger.LogError($"The query for path {path} and row {row} returned a null value.");
                continue;
            }

            var scenes = sceneSearchResponse.Data.ReturnedSceneData;
            var (predictionResults, errorMsg) = CalculatePrediction(path, row, numResults, scenes);
            if (predictionResults is null || !string.IsNullOrWhiteSpace(errorMsg))
            {
                m_logger.LogError(errorMsg);
                continue;
            }
            
            var predictedAcquisitionDateTimespan = predictionResults.PredictedAcquisitionDate - DateTime.Now;
            if (predictedAcquisitionDateTimespan.Ticks < 0)  
            {
                // If we're past the predicted acquisition date.
                // Because the next entry for the path & row could come (much) later.
                return;
            }
            
            if (predictedAcquisitionDateTimespan < m_maxNotificationOffset)
            {
                await NotifyUsers(path, row, predictionResults, targetsService, usersService, notificationSenderServices, userTargetNotificationService);
            }
        }
    }

    private async Task NotifyUsers(int path, int row, PredictionResults predictionResults,
        ITargetService targetsService, 
        IUserService usersService,
        List<INotificationSenderService> notificationSenderServices,
        DbUserTargetNotificationService userTargetNotificationService)
    {
        using var loggerScope = m_logger.BeginScope($"User notifications for path, row {path}, {row}.");
        m_logger.LogInformation($"Path, Row ({path}, {row}) detected to be under the max notification offset.");

        var predictedAcquisitionDateTimespan = predictionResults.PredictedAcquisitionDate - DateTime.Now;
        var userGuidsAndTargets = targetsService.GetPathAndRowData(path, row).ToList();

        foreach ((Guid userGuid, IEnumerable<Target> targets) in userGuidsAndTargets)
        {
            var userInfo = await usersService.TryGetUserByGuid(userGuid);

            if (userInfo is null)
            {
                m_logger.LogError($"Couldn't find user with guid \"{userGuid}\"");
                continue;
            }

            foreach (var target in targets)
            {
                if (predictedAcquisitionDateTimespan > target.NotificationOffset)
                {
                    continue;
                }
                
                var notificationRecord = userTargetNotificationService.GetNotificationRecord(path, row, userGuid, target.Guid, predictionResults.PredictedAcquisitionDate);
                if (notificationRecord is null || notificationRecord.HasBeenNotified)
                {
                    continue;
                }

                m_logger.LogInformation($"User \"{userInfo.Email}\" ({userInfo.Guid}) for target \"{target.Guid}\" " +
                                        $"(path, row: {target.Path}, {target.Row}) will be notified.");

                foreach (var notificationSenderService in notificationSenderServices)
                {
                    notificationSenderService.SendNotification(userInfo, target);
                }
                
                _ = userTargetNotificationService.SetNotificationStatus(path, row, userGuid, target.Guid, predictionResults.PredictedAcquisitionDate, true);
            }
        }
    }

    private static (PredictionResults?, string errorMsg) CalculatePrediction(int path, int row, int numResults, SceneData[] scenes)
    {
        if (scenes.Length < numResults)
        {
            string errorMsg = $"Skipped path & row: {path}, {row}. Returned {scenes.Length} < {numResults}.";
            return (null, errorMsg);
        }

        var getSatelliteFilter = (int satelliteNumber) => (SceneData sceneData) =>
        {
            var satelliteMetadata = MetadataExtensions.TryGetMetadataByName(sceneData.Metadata, "Satellite");
            return satelliteMetadata is not null && int.Parse(satelliteMetadata.Value) == satelliteNumber;
        };

        var sat8DateInfos = scenes.Where(getSatelliteFilter(8))
            .Select(PredictionService.ToSceneDateInfo)
            .ToArray();

        if (sat8DateInfos.Length <= 2)
        {
            string errorMsg = $"Skipped path & row: {path}, {row}. Not enough data for landsat 8, with count < 2.";
            return (null, errorMsg);
        }

        var sat9DateInfos = scenes.Where(getSatelliteFilter(9))
            .Select(PredictionService.ToSceneDateInfo)
            .ToArray();

        if (sat9DateInfos.Length < 2)
        {
            string errorMsg = $"Skipped path & row: {path}, {row}. Not enough data for landsat 9, with count < 2.";
            return (null, errorMsg);
        }

        var predictionResults = PredictionService.PredictCore(sat8DateInfos, sat9DateInfos);
        return (predictionResults, string.Empty);
    }
    
    private static SceneSearchRequest CreateSceneSearchRequest(int path, int row, int numResults)
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
        
        var metadataFilter = new MetadataFilterAnd
        {
            ChildFilters = [ pathFilter, rowFilter ]
        };

        var sceneFilter = new SceneFilter
        {
            MetadataFilter = metadataFilter
        };
        
        return new SceneSearchRequest
        {
            DatasetName = UsgsApiService.DatasetName,
            MaxResults = numResults,
            UseCustomization = false,
            SceneFilter = sceneFilter,
        };
    }
}