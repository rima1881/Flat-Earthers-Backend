using System.Reflection;
using System.Text.Json;
using LandsatReflectance.Backend.Models.UsgsApi;
using LandsatReflectance.Backend.Models.UsgsApi.Endpoints;
using LandsatReflectance.Backend.Models.UsgsApi.Types;
using LandsatReflectance.Backend.Models.UsgsApi.Types.Request;
using LandsatReflectance.Backend.Services.NotificationSender;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.Extensions.Options;
using PredictionService = LandsatReflectance.Backend.Services.UsgsDateTimePredictionService;

namespace LandsatReflectance.Backend.Services.BackgroundServices;

public class NotificationService : BackgroundService
{
    private readonly TimeSpan m_checkInterval = TimeSpan.FromMinutes(10);
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
        /*
        while (!stoppingToken.IsCancellationRequested)
        {
        }
         */
        
        await Perform();
        await Task.Delay(m_checkInterval, stoppingToken);
    }

    private async Task Perform()
    {
        using var serviceScope = m_serviceScopeFactory.CreateScope();
        var usgsApiService = serviceScope.ServiceProvider.GetRequiredService<UsgsApiService>();
        var usersService = serviceScope.ServiceProvider.GetRequiredService<IUserService>();
        var targetsService = serviceScope.ServiceProvider.GetRequiredService<ITargetService>();
        var notificationSenderServices = serviceScope.ServiceProvider.GetServices<INotificationSenderService>().ToList();

        foreach ((int path, int row) in targetsService.GetAllRegisteredPathAndRows())
        {
            m_logger.LogInformation($"Examining path & row: {path}, {row}.");

            const int numResults = 10;
            var sceneSearchRequest = CreateSceneSearchRequest(path, row, numResults);

            var sceneSearchResponse = await usgsApiService.QuerySceneSearch(sceneSearchRequest);

            if (sceneSearchResponse.Data is null)
            {
                m_logger.LogError("Some error has occurred.");
                continue;
            }

            var scenes = sceneSearchResponse.Data.ReturnedSceneData;

            if (scenes.Length < numResults)
            {
                m_logger.LogError($"Skipped path & row: {path}, {row}. This may be an invalid scene.");
                continue;
            }

            var sat8DateInfos = scenes.Where(scene =>
                {
                    var satelliteMetadata = MetadataExtensions.TryGetMetadataByName(scene.Metadata, "Satellite");
                    return satelliteMetadata is not null && int.Parse(satelliteMetadata.Value) == 8;
                })
                .Select(PredictionService.ToSceneDateInfo)
                .ToArray();

            if (sat8DateInfos.Length <= 2)
            {
                m_logger.LogError($"Skipped path & row: {path}, {row}. Not enough data for landsat 8, with count < 2.");
                continue;
            }

            var sat9DateInfos = scenes.Where(scene =>
                {
                    var satelliteMetadata = MetadataExtensions.TryGetMetadataByName(scene.Metadata, "Satellite");
                    return satelliteMetadata is not null && int.Parse(satelliteMetadata.Value) == 9;
                })
                .Select(PredictionService.ToSceneDateInfo)
                .ToArray();

            if (sat9DateInfos.Length < 2)
            {
                m_logger.LogError($"Skipped path & row: {path}, {row}. Not enough data for landsat 9, with count < 2.");
                continue;
            }

            var predictionResults = PredictionService.PredictCore(sat8DateInfos, sat9DateInfos);

            var predictedAcquisitionDateTimespan = predictionResults.PredictedAcquisitionDate - DateTime.Now;
            if (predictedAcquisitionDateTimespan < m_maxNotificationOffset)
            {
                m_logger.LogInformation($"Path, Row ({path}, {row}) detected to be under the max notification offset.");

                int pathCopy = path;
                int rowCopy = row;
                var userGuidsAndTargets = targetsService
                    .GetLinkedUsers(target => target.Path == pathCopy && target.Row == rowCopy)
                    .ToList();

                foreach (var tuple in userGuidsAndTargets)
                {
                    var userGuid = tuple.Item1;
                    var userInfo = await usersService.TryGetUserByGuid(userGuid);

                    if (userInfo is null)
                    {
                        m_logger.LogError($"Couldn't find user with guid \"{userGuid}\"");
                        continue;
                    }

                    foreach (var target in tuple.Item2)
                    {
                        if (predictedAcquisitionDateTimespan > target.NotificationOffset)
                        {
                            continue;
                        }

                        m_logger.LogInformation(
                            $"User \"{userInfo.Email}\" ({userInfo.Guid}) for target \"{target.Guid}\" " +
                            $"(path, row: {target.Path}, {target.Row}) will be notified.");

                        foreach (var notificationSenderService in notificationSenderServices)
                        {
                            notificationSenderService.SendNotification(userInfo, target);
                        }
                    }
                }
            }
        }
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
        /*
        var satelliteFilter = new MetadataFilterOr
        {
            ChildFilters =
            [
                new MetadataFilterValue
                {
                    FilterId = "5e83d14ff1eda1b8",
                    Value = "8",
                    Operand = MetadataFilterValue.MetadataValueOperand.Equals
                },
                new MetadataFilterValue
                {
                    FilterId = "5e83d14ff1eda1b8",
                    Value = "9",
                    Operand = MetadataFilterValue.MetadataValueOperand.Equals
                }
            ]
        };
         */
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