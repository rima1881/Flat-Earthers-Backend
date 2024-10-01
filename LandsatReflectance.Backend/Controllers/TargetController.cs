using LandsatReflectance.Backend.Models;
using LandsatReflectance.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LandsatReflectance.Backend.Controllers;

[ApiController]
[Route("")]
public class TargetController : ControllerBase
{
    private ILogger<TargetController> m_logger;
    private IUserService m_userService;
    private ITargetService m_targetsService;
    
    public TargetController(ILogger<TargetController> logger, IUserService userService, ITargetService targetService)
    {
        m_logger = logger;
        m_userService = userService;
        m_targetsService = targetService;
    }


    public class AddTargetsRequest
    {
        public string Email { get; set; } = String.Empty;
        public Target[] Targets { get; set; } = [];
    }

    [HttpPost("AddTargets", Name = "AddTargets")]
    public IActionResult AddTargets([FromBody] AddTargetsRequest addTargetsRequest)
    {
        var user = m_userService.TryGetUser(addTargetsRequest.Email);
        if (user is null)
        {
            return BadRequest($"Could not find the user with email \"{addTargetsRequest.Email}\".");
        }

        m_targetsService.AddTargets(addTargetsRequest.Targets.Select(target => (user, target)));
        return Ok(addTargetsRequest.Targets.Select(target => target.Guid));
    }


    [HttpGet("GetTargets", Name = "GetTargets")]
    public IActionResult GetTargets([FromQuery(Name = "email")] string email)
    {
        var user = m_userService.TryGetUser(email);
        if (user is null)
        {
            return BadRequest($"Could not find the user with email \"{email}\".");
        }

        return Ok(m_targetsService.GetTargets(_ => true, guid => user.Guid == guid));
    }

    [HttpDelete("DeleteTarget", Name = "DeleteTarget")]
    public IActionResult DeleteTarget(
        [FromQuery(Name = "email")] string email, 
        [FromQuery(Name = "guid")] Guid targetGuid)
    {
        var user = m_userService.TryGetUser(email);
        if (user is null)
        {
            return BadRequest($"Could not find the user with email \"{email}\".");
        }
        
        var removedTarget = m_targetsService.TryRemoveTarget(target => target.Guid == targetGuid, guid => user.Guid == guid);
        
        if (removedTarget is null)
        {
            return BadRequest($"Could not remove the target with id \"{targetGuid}\" bound to user \"{email}\".");
        }

        return Ok();
    }
}