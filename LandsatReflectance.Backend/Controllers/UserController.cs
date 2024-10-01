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
        var selectedUser = m_userService.TryGetUser(email);

        if (selectedUser is not null)
            return Ok(selectedUser);

        if (!IsValidEmail(email))
            return BadRequest($"\"{email}\" is not a valid email");

        var newUser = new User
        {
            Email = email,
            Targets = []
        };
        m_userService.AddUser(newUser);
        
        return Ok(newUser);
    }

    
    public class RegisterUserRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public Target[] Targets { get; set; } = [];
    } 
    
    public IActionResult RegisterUser([FromBody] RegisterUserRequest registerUserRequest)
    {
        throw new NotImplementedException();
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