using System.Security.Claims;
using System.Text.Json;
using LandsatReflectance.Backend.Models;
using LandsatReflectance.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace LandsatReflectance.Backend.Controllers;

#if !DISABLE_AUTH
[Authorize]
#endif
[ApiController]
[Route("")]
public class TargetController : ControllerBase
{
    private ILogger<TargetController> m_logger;
    private JsonSerializerOptions m_jsonSerializerOptions;
    
    private IUserService m_userService;
    private ITargetService m_targetsService;
    
    public TargetController(ILogger<TargetController> logger, IOptions<JsonOptions> jsonOptions, IUserService userService, ITargetService targetService)
    {
        m_logger = logger;
        m_jsonSerializerOptions = jsonOptions.Value.JsonSerializerOptions;
        
        m_userService = userService;
        m_targetsService = targetService;
    }


    public class AddTargetsRequest
    {
        public string Email { get; set; } = String.Empty;
        public Target[] Targets { get; set; } = [];
    }

    [HttpPost("AddTargets", Name = "AddTargets")]
    public async Task<IActionResult> AddTargets([FromBody] AddTargetsRequest addTargetsRequest)
    {
        var identity = HttpContext.User.Identity as ClaimsIdentity;
        (User? user, string errorMsg) = await AuthenticateToken(identity);
        if (user is null)
        {
            return Unauthorized(errorMsg);
        }

        m_targetsService.AddTargets(addTargetsRequest.Targets.Select(target => (user, target)));
        return Ok(addTargetsRequest.Targets.Select(target => target.Guid));
    }


    [HttpGet("GetTargets", Name = "GetTargets")]
    public async Task<IActionResult> GetTargets([FromQuery(Name = "email")] string _)
    {
        var identity = HttpContext.User.Identity as ClaimsIdentity;
        (User? user, string errorMsg) = await AuthenticateToken(identity);
        if (user is null)
        {
            return Unauthorized(errorMsg);
        }

        return Ok(m_targetsService.GetTargets(_ => true, guid => user.Guid == guid));
    }

    [HttpDelete("DeleteTarget", Name = "DeleteTarget")]
    public async Task<IActionResult> DeleteTarget(
        [FromQuery(Name = "email")] string email, 
        [FromQuery(Name = "guid")] Guid targetGuid)
    {
        var identity = HttpContext.User.Identity as ClaimsIdentity;
        (User? user, string errorMsg) = await AuthenticateToken(identity);
        if (user is null)
        {
            return Unauthorized(errorMsg);
        }
        
        var removedTarget = m_targetsService
            .TryRemoveTarget(target => target.Guid == targetGuid, guid => user.Guid == guid)
            .ToList();
        
        if (removedTarget.Count == 0)
        {
            return BadRequest($"Could not remove the target with id \"{targetGuid}\" bound to user \"{email}\".");
        }

        return Ok();
    }
    
    
    public class EditTargetRequest 
    {
        public Guid TargetGuid { get; set; }
        public double? NewMinCloudCover { get; set; }
        public double? NewMaxCloudCover { get; set; }
        public TimeSpan? NewNotificationOffset { get; set; }
    }

    [HttpPost("EditTarget", Name = "EditTarget")]
    public async Task<IActionResult> EditTargetInfo([FromBody] EditTargetRequest editTargetRequest)
    {
        var identity = HttpContext.User.Identity as ClaimsIdentity;
        (User? user, string errorMsg) = await AuthenticateToken(identity);
        if (user is null)
        {
            return Unauthorized(errorMsg);
        }

        Action<Target> editTarget = target =>
        {
            target.MinCloudCover = editTargetRequest.NewMinCloudCover ?? target.MinCloudCover;
            target.MaxCloudCover = editTargetRequest.NewMaxCloudCover ?? target.MaxCloudCover;
            target.NotificationOffset = editTargetRequest.NewNotificationOffset ?? target.NotificationOffset;
        };
        
        var editedTargets = m_targetsService.TryEditTarget(editTarget, 
            target => target.Guid == editTargetRequest.TargetGuid, 
            guid => guid == user.Guid)
            .ToList();

        if (editedTargets.Count != 1)
        {
            return BadRequest();
        }

        return Ok(JsonSerializer.Serialize(editedTargets[0], m_jsonSerializerOptions));
    }
    
    
    private async Task<(User? user, string errorMsg)> AuthenticateToken(ClaimsIdentity? identity)
    {
        if (identity is null)
        {
#if DEBUG
            return (null, "No claim was provided");
#else
            return "";
#endif
        }

        // asp.net core being a bitch and won't let me disable mapping for some jwt registered claim names
        var userGuidClaim = identity.FindFirst("sub")?.Value
            ?? identity.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
        
        var userEmailClaim = identity.FindFirst("email")?.Value
            ?? identity.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value;
        
        var hashedPasswordClaim = identity.FindFirst("hashed_password")?.Value;

        if (userGuidClaim is null || userEmailClaim is null || hashedPasswordClaim is null)
        {
#if DEBUG
            return (null, "One of the claims \"Sub\", \"Email\", \"HashedPassword\" is missing");
#else
            return (null, String.Empty);
#endif
        }

        var user = await m_userService.TryGetUser(userEmailClaim);
        if (user is null)
        {
#if DEBUG
            return (null, $"Could not find user with email \"{userEmailClaim}\"");
#else
            return (null, String.Empty);
#endif
        }

        if (user.Guid != Guid.Parse(userGuidClaim))
        {
#if DEBUG
            return (null, "The guid claim at \"Sub\" was not valid.");
#else
            return (null, String.Empty);
#endif
        }
        
        if (!string.Equals(user.PasswordHash.Trim(), hashedPasswordClaim.Trim()))
        {
#if DEBUG
            return (null, "The hashed password claim at \"HashedPassword\" was not valid.");
#else
            return (null, String.Empty);
#endif
        }

        return (user, String.Empty);
    }
}