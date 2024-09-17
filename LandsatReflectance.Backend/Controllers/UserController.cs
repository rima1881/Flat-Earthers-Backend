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
        return selectedUser is not null 
            ? Ok(selectedUser) 
            : Ok(Array.Empty<User>());
    }
}