using System.Text.RegularExpressions;
using LandsatReflectance.Backend.Models;
using LandsatReflectance.Backend.Services;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace LandsatReflectance.Backend.Controllers;

[ApiController]
[Route("[controller]")]
public class UserController : ControllerBase
{
    private readonly UserService UserService;
    
    public UserController(UserService userService)
    {
        UserService = userService;
    }


    [HttpGet]
    [SwaggerOperation(Summary = "Gets user information")]
    public IActionResult GetUserInfo(
        [FromQuery(Name = "email")] string email = "")
    {
        var selectedUser = UserService.Users.FirstOrDefault(user => string.Equals(user.Email, email));

        if (selectedUser is not null)
            return Ok(selectedUser);

        if (!IsValidEmail(email))
            return BadRequest($"\"{email}\" is not a valid email");

        var newUser = new User
        {
            Email = email,
            SelectedRegions = []
        };
        UserService.Users.Add(newUser);
        
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