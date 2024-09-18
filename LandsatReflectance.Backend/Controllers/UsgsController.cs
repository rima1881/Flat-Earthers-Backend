using LandsatReflectance.Backend.Models.UsgsApi.Endpoints;
using LandsatReflectance.Backend.Models.UsgsApi.Types.Request;
using LandsatReflectance.Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
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
        // Acquisition filter
        var intervalDuration = new TimeSpan(30, 0, 0, 0);
        var acquisitionFilter = new AcquisitionFilter
        {
            Start = DateTime.Now - intervalDuration,
            End = DateTime.Now
        };

        // Metadata filter
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
            AcquisitionFilter = acquisitionFilter,
            MetadataFilter = metadataFilter
        };
        
        
        // Scene search
        var sceneSearchRequest = new SceneSearchRequest
        {
            DatasetName = "landsat_ot_c2_l2",
            MaxResults = 50,
            UseCustomization = false,
            SceneFilter = sceneFilter,
        };


        var response = await UsgsApiService.QuerySceneSearch(sceneSearchRequest);

        var data = response.Data;
        if (data is null)
            return BadRequest();

        var browsePaths =
            data.ReturnedSceneData
                .Where(sceneData => sceneData.BrowseInfos.Length > 0)
                .Select(sceneData => sceneData.BrowseInfos[0].BrowsePath);
        
        return Ok(browsePaths);
    }
    
    [HttpGet("Prediction", Name = "Prediction")]
    [SwaggerOperation(Summary = "Returns information about the next predicted acquisition time for an image/scene.")]
    public IActionResult GetNextAcquisitionPrediction(
        [FromQuery(Name = "path")] int path, 
        [FromQuery(Name = "row")] int row)
    {
        return Ok();
    }
}