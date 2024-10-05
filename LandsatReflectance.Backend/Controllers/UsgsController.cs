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

#if !DEBUG
[Authorize]
#endif
[ApiController]
[Route("")]
[SwaggerTag("This controller manages interactions with the USGS m2m API.")]
public class UsgsController : ControllerBase
{
    private const string DatasetName = "landsat_ot_c2_l2";
    
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
        var sceneDataArr = await PerformSceneSearch(path, row, numResults);
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

    
    
#region Helper Methods

    /// <remarks>
    /// We can 'save' a list of scene 'entityId' strings on the usgs m2m api side using the endpoint 'scene-list-...'.
    /// <br></br>
    /// What we're doing right here is checking if we already have something in that list and returning it.
    /// </remarks>
    private async Task<string[]?> TryGetEntityIdListFromOnlineSave(int path, int row)
    {
        // TODO: Add extra checks where we ensure that the number of returned data from 'scene-list-get' equals 'numResults'
        var sceneListGetRequest = new SceneListGetRequest
        {
            ListId = SceneEntityIdCachingService.PathAndRowToCacheKey(path, row),
            DatasetName = DatasetName,
            StartingNumber = 0,
            MaxResults = 1000,
        };

        var sceneListGetResponse = await m_usgsApiService.QuerySceneListGet(sceneListGetRequest);
        return sceneListGetResponse.Data?.EntityIds;
    }

    private async Task<SceneData[]?> PerformSceneSearch(int path, int row, int numResults)
    {
        var sceneSearchRequest = CreateSceneSearchRequest(path, row, numResults);
        var sceneSearchResponse = await m_usgsApiService.QuerySceneSearch(sceneSearchRequest);

        var sceneSearchData = sceneSearchResponse.Data;
        return sceneSearchData?.ReturnedSceneData.ToArray();
    }

    private async Task<(bool isUnsuccessful, string errorMsg)> TryWriteToOnlineSave(int path, int row, string[] entityIds)
    {
        var sceneListAddRequest = new SceneListAddRequest
        {
            ListId = SceneEntityIdCachingService.PathAndRowToCacheKey(path, row),
            DatasetName = DatasetName,
            IdField = "entityId",
            EntityIds = entityIds,
            TimeToLive = "P1M", // Stored for a month
            CheckDownloadRestriction = false,
        };
        var sceneListAddResponse = await m_usgsApiService.QuerySceneListAdd(sceneListAddRequest);
        
        if (sceneListAddResponse.Data is null)
            return (true, "");

        return sceneListAddResponse.Data.ListLength == 0
            ? (true, "")
            : (false, "");
    }
    
#endregion
    


#region Static Helper Methods

    private static SceneSearchRequest CreateSceneSearchRequest(int path, int row, int numResults)
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
            DatasetName = DatasetName,
            MaxResults = numResults,
            UseCustomization = false,
            SceneFilter = sceneFilter,
        };
    }
    
#endregion
}