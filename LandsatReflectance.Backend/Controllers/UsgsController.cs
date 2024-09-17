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
    public IActionResult GetImages(
        [FromQuery(Name = "path")] int path, 
        [FromQuery(Name = "row")] int row)
    {
        return Ok();
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