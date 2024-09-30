using System.Text.RegularExpressions;
using LandsatReflectance.Backend.Models;
using LandsatReflectance.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace LandsatReflectance.Backend.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService m_userService;
    
    public UserController(IUserService userService)
    {
        m_userService = userService;
    }


    [HttpGet]
    [SwaggerOperation(Summary = "Gets user information")]
    public IActionResult GetUserInfo(
        [FromQuery(Name = "email")] string email = "")
    {
        var selectedUser = m_userService.GetUser(email);

        if (selectedUser is not null)
            return Ok(selectedUser);

        if (!IsValidEmail(email))
            return BadRequest($"\"{email}\" is not a valid email");

        var newUser = new User
        {
            Email = email,
            SelectedRegions = []
        };
        m_userService.AddUser(newUser);
        
        return Ok(newUser);
    }
    
    // chatgpt code fr fr
    private static bool IsValidEmail(string str)
    {
        if (string.IsNullOrWhiteSpace(str))
            return false;

        string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(str, pattern, RegexOptions.IgnoreCase);
    }
}