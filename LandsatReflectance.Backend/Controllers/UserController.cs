using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using LandsatReflectance.Backend.Models;
using LandsatReflectance.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Annotations;

namespace LandsatReflectance.Backend.Controllers;

[ApiController]
[Route("")]
public class UserController : ControllerBase
{
    private readonly IUserService m_userService;
    private readonly KeysService m_keysService;
    
    public UserController(IUserService userService, KeysService keysService)
    {
        m_userService = userService;
        m_keysService = keysService;
    }


    public class RegisterUserRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    } 
    
    [HttpPost("Register", Name = "Register")]
    public IActionResult RegisterUser([FromBody] RegisterUserRequest registerUserRequest)
    {
        var passwordHasher = new PasswordHasher<string>();
        var user = new User
        {
            Email = registerUserRequest.Email,
            PasswordHash = passwordHasher.HashPassword(registerUserRequest.Email, registerUserRequest.Password),
        };
        
        m_userService.AddUser(user);
        
        
        // TODO: Change this later
        
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(m_keysService.AuthSecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, registerUserRequest.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        
        // TODO: 2. Add issuer and audience down the line

        var token = new JwtSecurityToken(
            claims: claims, 
            expires: DateTime.UtcNow.AddHours(1), 
            signingCredentials: credentials
            );

        var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

        if (string.IsNullOrWhiteSpace(jwtToken))
        {
            throw new AuthenticationException("Failed to generate authentication token. Generated an null or empty string.");
        }

        var something = new Dictionary<string, object>
        {
            { "user", user },
            { "token", jwtToken },
        };
        
        return Ok(something);
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