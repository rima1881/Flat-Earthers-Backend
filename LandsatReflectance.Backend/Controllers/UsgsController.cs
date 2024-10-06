using System.Text.Json;
using System.Text.Json.Serialization;
using LandsatReflectance.Backend.Models.UsgsApi;
using LandsatReflectance.Backend.Models.UsgsApi.Endpoints;
using LandsatReflectance.Backend.Models.UsgsApi.Types.Request;
using LandsatReflectance.Backend.Services;
using LandsatReflectance.Backend.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Annotations;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

namespace LandsatReflectance.Backend.Controllers;

[ApiController]
[Route("")]
[SwaggerTag("This controller manages interactions with the USGS m2m API.")]
public class UsgsController : ControllerBase
{
    private readonly UsgsApiService m_usgsApiService;
    private readonly SceneEntityIdCachingService m_sceneEntityIdCachingService;
    private readonly JsonSerializerOptions m_jsonSerializerOptions;
    
    
    public UsgsController(UsgsApiService usgsApiService, SceneEntityIdCachingService sceneEntityIdCachingService, IOptions<JsonOptions> jsonOptions)
    {
        m_usgsApiService = usgsApiService;
        m_sceneEntityIdCachingService = sceneEntityIdCachingService;
        m_jsonSerializerOptions = jsonOptions.Value.SerializerOptions;
    }
    
    
    [HttpGet("Images", Name = "Images")]
    [SwaggerOperation(Summary = "Returns image/scene information.")]
    public async Task<IActionResult> GetImages(
        [FromServices] IOptionsSnapshot<JsonOptions> jsonOptions,
        [FromQuery(Name = "path")] int path, 
        [FromQuery(Name = "row")] int row,
        [FromQuery(Name = "numResults")] int numResults = 5)
    {
        var sceneSearchRequest = CreatePathRowSceneSearchRequest(path, row, numResults);
        var sceneSearchResponse = await m_usgsApiService.QuerySceneSearch(sceneSearchRequest);

        var sceneSearchData = sceneSearchResponse.Data;
        var sceneDataArr = sceneSearchData?.ReturnedSceneData.ToArray();
        if (sceneDataArr is null)
            return BadRequest();

        var jsonSerializerOptions = jsonOptions.Value.SerializerOptions;
        jsonSerializerOptions.Converters.Insert(0, new SceneDataSimplifiedConverter());

        return Content(JsonSerializer.Serialize(sceneDataArr.OrderByDescending(sceneData => sceneData.PublishDate), jsonSerializerOptions));
    }
    
    [HttpGet("Prediction", Name = "Prediction")]
    [SwaggerOperation(Summary = "Returns information about the next predicted acquisition time for an image/scene.")]
    public async Task<IActionResult> GetNextAcquisitionPrediction(
        [FromQuery(Name = "path")] int path, 
        [FromQuery(Name = "row")] int row)
    {
        return Ok(await UsgsDateTimePredictionService.Predict(m_usgsApiService, path, row));
    }

    [HttpGet("Pixels", Name = "Pixels")]
    public async Task<IActionResult> GetPixelGrid(
        [FromServices] IOptionsSnapshot<JsonOptions> jsonOptions,
        [FromQuery(Name = "entityId")] string entityId, 
        [FromQuery(Name = "latitude")] double latitude, 
        [FromQuery(Name = "longitude")] double longitude, 
        [FromQuery(Name = "zoomLevel")] int zoomLevel)
    {
        var sceneSearchRequest = CreateByEntityIdSceneSearchRequest(entityId);
        var sceneSearchResponse = await m_usgsApiService.QuerySceneSearch(sceneSearchRequest);

        var sceneSearchData = sceneSearchResponse.Data;
        var sceneDataArr = sceneSearchData?.ReturnedSceneData.ToArray();
        if (sceneDataArr is null)
            return BadRequest();

        if (sceneDataArr.Length != 1)
            return BadRequest();

        SceneData sceneData = sceneDataArr[0];
        
        if (sceneData.BrowseInfos.Length != 1)
            return BadRequest();

        var overlayPathTemplate = sceneData.BrowseInfos[0].OverlayPath;
        var toReturn = new List<string>();

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                (int x, int y) = GetTileCoordinates(latitude, longitude, zoomLevel);
                toReturn.Add(overlayPathTemplate
                    .Replace("{z}", zoomLevel.ToString())
                    .Replace("{x}", (x + i).ToString())
                    .Replace("{y}", (y + j).ToString())
                );
            }
        }

        return Ok(toReturn);
    }
    
    

    
    
#region Helper Methods
    public static (int x, int y) GetTileCoordinates(double latitude, double longitude, int zoomLevel)
    {
        // Convert latitude and longitude to radians
        double latRad = latitude * Math.PI / 180;

        // Calculate the tile numbers
        int x = (int)Math.Floor((longitude + 180.0) / 360.0 * Math.Pow(2, zoomLevel));
        int y = (int)Math.Floor((1.0 - Math.Log(Math.Tan(latRad) + 1.0 / Math.Cos(latRad)) / Math.PI) / 2.0 * Math.Pow(2, zoomLevel));

        return (x, y);
    }

    /*
    private async Task<SceneData[]?> PerformSceneSearch(int path, int row, int numResults)
    {
        var sceneSearchRequest = CreatePathRowSceneSearchRequest(path, row, numResults);
        var sceneSearchResponse = await m_usgsApiService.QuerySceneSearch(sceneSearchRequest);

        var sceneSearchData = sceneSearchResponse.Data;
        return sceneSearchData?.ReturnedSceneData.ToArray();
    }
     */
#endregion
    


#region Static Helper Methods

    private static SceneSearchRequest CreatePathRowSceneSearchRequest(int path, int row, int numResults)
    {
        var metadataFilter = new MetadataFilterAnd 
        {
            ChildFilters = [
                new MetadataFilterValue  // Path filer
                {
                    FilterId = "5e83d14fb9436d88",
                    Value = path.ToString(),
                    Operand = MetadataFilterValue.MetadataValueOperand.Equals
                },
                new MetadataFilterValue  // Row filter
                {
                    FilterId = "5e83d14ff1eda1b8",
                    Value = row.ToString(),
                    Operand = MetadataFilterValue.MetadataValueOperand.Equals
                }
            ] 
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

    private static SceneSearchRequest CreateByEntityIdSceneSearchRequest(string entityId)
    {
        var metadataFilter = new MetadataFilterAnd 
        {
            ChildFilters = [
                new MetadataFilterValue  // Path filer
                {
                    FilterId = "5e83d14fc84c9a78",
                    Value = entityId, 
                    Operand = MetadataFilterValue.MetadataValueOperand.Equals
                }
            ] 
        };
        
        var sceneFilter = new SceneFilter
        {
            MetadataFilter = metadataFilter
        };
        
        return new SceneSearchRequest
        {
            DatasetName = UsgsApiService.DatasetName,
            MaxResults = 1,
            UseCustomization = false,
            SceneFilter = sceneFilter,
        };
    }
    
#endregion
}