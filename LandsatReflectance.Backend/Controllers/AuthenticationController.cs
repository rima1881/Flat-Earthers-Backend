using System.Buffers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using System.Security.Claims;
using System.Text;
using LandsatReflectance.Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.Annotations;

namespace LandsatReflectance.Backend.Controllers;

[ApiController]
[Route("")]
public class AuthenticationController : ControllerBase
{
    private readonly string m_authSecretKey;
    
    public AuthenticationController(KeysService keysService)
    {
        m_authSecretKey = keysService.AuthSecretKey;
    }

    [HttpPost("Login", Name = "Login")]
    public IActionResult Login(
        [FromQuery(Name = "email")] string email = "",
        [FromQuery(Name = "password")] string password = "")
    {
        // TODO: 1. Check if the credentials if they match whatever we have in the database.

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(m_authSecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

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
        
        return Ok(new { Token = jwtToken });
    }
}