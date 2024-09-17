using LandsatReflectance.Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace LandsatReflectance.Backend.Controllers;

[ApiController]
[Route("[controller]")]
public class UsgsController : ControllerBase
{
    private readonly UsgsApiService UsgsApiService;
    
    public UsgsController(UsgsApiService usgsApiService)
    {
        UsgsApiService = usgsApiService;
    }
    
    /*
    [HttpGet]
    public async Task<IActionResult> GetImages(int path, int row)
    {
        return Ok();
    }
     */
    
    [HttpGet(Name = "GetNextAcquisitionPrediction")]
    public Task<IActionResult> GetNextAcquisitionPrediction(int path, int row)
    {
        return Task.FromResult<IActionResult>(Ok());
    }
}