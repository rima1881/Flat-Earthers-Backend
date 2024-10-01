using LandsatReflectance.Backend.Models;
using LandsatReflectance.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LandsatReflectance.Backend.Controllers;

[Authorize]
[ApiController]
[Route("")]
public class TargetController
{
    private ILogger<TargetController> m_logger;
    private IUserService m_userService;
    
    public TargetController(ILogger<TargetController> logger, IUserService userService)
    {
        m_logger = logger;
        m_userService = userService;
    }


    public class AddTargetsRequest
    {
        public string Email { get; set; } = String.Empty;
        public Target[] Targets { get; set; } = [];
    }

    [HttpPost("AddTargets", Name = "AddTargets")]
    public IActionResult AddTargets([FromBody] AddTargetsRequest addTargetsRequest)
    {
        throw new NotImplementedException();
    }


    [HttpGet("GetTargets", Name = "GetTargets")]
    public IActionResult GetTargets([FromQuery(Name = "email")] string email)
    {
        throw new NotImplementedException();
    }

    [HttpDelete("DeleteTarget", Name = "DeleteTarget")]
    public IActionResult DeleteTarget(
        [FromQuery(Name = "email")] string email, 
        [FromQuery(Name = "path")] int path, 
        [FromQuery(Name = "row")] int row)
    {
        throw new NotImplementedException();
    }
}