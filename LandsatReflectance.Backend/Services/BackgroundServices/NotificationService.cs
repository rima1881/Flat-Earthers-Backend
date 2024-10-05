using System.Reflection;
using System.Text.Json;
using LandsatReflectance.Backend.Models.UsgsApi;
using LandsatReflectance.Backend.Models.UsgsApi.Endpoints;
using LandsatReflectance.Backend.Models.UsgsApi.Types;
using LandsatReflectance.Backend.Models.UsgsApi.Types.Request;
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

    /*
    private async Task Perform(CancellationToken stoppingToken)
    {
        using var serviceScope = m_serviceScopeFactory.CreateScope();
        var usgsApiService = serviceScope.ServiceProvider.GetRequiredService<UsgsApiService>();
        
        try
        {
            
            // const int maxPathInclusive = 233;

            var assemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? "";
            var outputPath = Path.Join(assemblyPath, @"Test\pathRowData.json");
            var someDictionary = new Dictionary<string, int>();

            int path = 1;
            do
            {
                m_logger.LogInformation($"Querying path: {path}");
                
                try
                {
                    var sceneSearchRequest = CreateSceneSearchRequest(path, 1000);
                    var sceneSearchResponse = await usgsApiService.QuerySceneSearch(sceneSearchRequest);
                    
                    var scenes = sceneSearchResponse.Data?.ReturnedSceneData ?? [];
                    var rowCount = scenes
                        .GroupBy(TryGetRow)
                        .Where(grouping => grouping.Key is not null)
                        .Select(grouping => (row: int.Parse(grouping.Key!), sceneCount: grouping.Count()))
                        .OrderBy(tuple => tuple.row)
                        .ToList();

                    if (rowCount.Count == 0)
                    {
                        Environment.Exit(0);
                    }

                    rowCount.ForEach(tuple => someDictionary[$"{path}, {tuple.row}"] = tuple.sceneCount);
                    await File.WriteAllTextAsync(outputPath, JsonSerializer.Serialize(someDictionary, m_jsonSerializerOptions), stoppingToken);
                    m_logger.LogInformation($"Finished querying path: {path}");
                }
                catch (Exception exception)
                {
                    m_logger.LogError($"There was an error querying the path: {path}; with exception {exception.Message}.");
                }
                finally
                {
                    path++;
                }
                
            } while (true);
        }
        
        catch (OperationCanceledException)
        {

        }
        catch (Exception exception)
        {
        }
    }
     */

    private async Task Perform()
    {
        using var serviceScope = m_serviceScopeFactory.CreateScope();
        var usgsApiService = serviceScope.ServiceProvider.GetRequiredService<UsgsApiService>();
        var usersService = serviceScope.ServiceProvider.GetRequiredService<IUserService>();
        var targetsService = serviceScope.ServiceProvider.GetRequiredService<ITargetService>();
        
        for (var path = 1; path <= 233; path++)
        {
            for (var row = 1; row <= 248; row++)
            {
                var sceneSearchRequest = CreateSceneSearchRequest(path, row, 10);
                
                var sceneSearchResponse = await usgsApiService.QuerySceneSearch(sceneSearchRequest);

                if (sceneSearchResponse.Data is null)
                {
                    m_logger.LogError("Some error has occurred.");
                    continue;
                }

                var scenes = sceneSearchResponse.Data.ReturnedSceneData;

                var sat8DateInfos = scenes.Where(scene =>
                {
                    var satelliteMetadata = MetadataExtensions.TryGetMetadataByName(scene.Metadata, "Satellite");
                    return satelliteMetadata is not null && int.Parse(satelliteMetadata.Value) == 8;
                })
                    .Select(PredictionService.ToSceneDateInfo)
                    .ToArray();
                
                var sat9DateInfos = scenes.Where(scene =>
                {
                    var satelliteMetadata = MetadataExtensions.TryGetMetadataByName(scene.Metadata, "Satellite");
                    return satelliteMetadata is not null && int.Parse(satelliteMetadata.Value) == 9;
                })
                    .Select(PredictionService.ToSceneDateInfo)
                    .ToArray();

                var predictionResults = PredictionService.PredictCore(sat8DateInfos, sat9DateInfos);

                if (predictionResults.PredictedAcquisitionDate - DateTime.Now < m_maxNotificationOffset)
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
                            if (predictionResults.PredictedAcquisitionDate - DateTime.Now < target.NotificationOffset)
                            {
                                m_logger.LogInformation($"User \"{userInfo.Email}\" ({userInfo.Guid}) for target \"{target.Guid}\" " +
                                                        $"(path, row: {target.Path}, {target.Row}) will be notified.");
                            }
                        }
                    }
                    
                    var users = userGuidsAndTargets.Select(tuple => tuple.Item1).Select(userGuid => usersService.TryGetUserByGuid(userGuid).Result);
                }
            }
        }
        
        
    }

    private static string? TryGetRow(SceneData sceneData)
    {
        return sceneData.Metadata.FirstOrDefault(metadata => string.Equals(metadata.FieldName, "WRS Row"))?.Value;
    }
    
    /*
    private static SceneSearchRequest CreateSceneSearchRequestAlt(int path, int numResults)
    {
        var metadataFilter =
            new MetadataFilterValue
            {
                FilterId = "5e83d14fb9436d88",
                Value = path.ToString(),
                Operand = MetadataFilterValue.MetadataValueOperand.Equals
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
     */

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
        var metadataFilter = new MetadataFilterAnd
        {
            ChildFilters = [ pathFilter, rowFilter, satelliteFilter ]
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