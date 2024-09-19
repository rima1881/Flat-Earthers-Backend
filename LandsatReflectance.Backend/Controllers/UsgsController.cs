using System.Text.Json;
using System.Text.Json.Serialization;
using LandsatReflectance.Backend.Models.UsgsApi;
using LandsatReflectance.Backend.Models.UsgsApi.Endpoints;
using LandsatReflectance.Backend.Models.UsgsApi.Types.Request;
using LandsatReflectance.Backend.Services;
using LandsatReflectance.Backend.Utils;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace LandsatReflectance.Backend.Controllers;

[ApiController]
[Route("")]
[SwaggerTag("This controller manages interactions with the USGS m2m API.")]
public class UsgsController : ControllerBase
{
    private readonly UsgsApiService UsgsApiService;
    
    public UsgsController(UsgsApiService usgsApiService)
    {
        UsgsApiService = usgsApiService;
    }
    
    [HttpGet("Images", Name = "Images")]
    [SwaggerOperation(Summary = "Returns image/scene information.")]
    public async Task<IActionResult> GetImages(
        [FromQuery(Name = "path")] int path, 
        [FromQuery(Name = "row")] int row)
    {
        // Creating the request
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
        
        var sceneSearchRequest = new SceneSearchRequest
        {
            DatasetName = "landsat_ot_c2_l2",
            MaxResults = 5,
            UseCustomization = false,
            SceneFilter = sceneFilter,
        };


        // Query endpoint & process results
        var response = await UsgsApiService.QuerySceneSearch(sceneSearchRequest);

        var data = response.Data;
        if (data is null)
            return BadRequest();

        var jsonSerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new SceneDataSimplifiedConverter() }
        };

        var browsePaths =
            data.ReturnedSceneData
                .Where(sceneData => sceneData.BrowseInfos.Length > 0)
                .ToArray();

        return Content(JsonSerializer.Serialize(browsePaths, jsonSerializerOptions));
    }
    
    [HttpGet("Prediction", Name = "Prediction")]
    [SwaggerOperation(Summary = "Returns information about the next predicted acquisition time for an image/scene.")]
    public async Task<IActionResult> GetNextAcquisitionPrediction(
        [FromQuery(Name = "path")] int path, 
        [FromQuery(Name = "row")] int row)
    {
        return Ok(await UsgsDateTimePredictionService.Predict(UsgsApiService, path, row));
    }
}