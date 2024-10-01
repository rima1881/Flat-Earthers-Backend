using System.Text;
using System.Security.Claims;
using System.Security.Authentication;
using System.IdentityModel.Tokens.Jwt;

using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

using LandsatReflectance.Backend.Services;
using Microsoft.AspNetCore.Authorization;


namespace LandsatReflectance.Backend.Controllers;

[Authorize]
[ApiController]
[Route("")]
public class AuthenticationController : ControllerBase
{
    private readonly string m_authSecretKey;
    
    public AuthenticationController(KeysService keysService)
    {
        m_authSecretKey = keysService.AuthSecretKey;
    }


    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    [HttpPost("Login", Name = "Login")]
    public IActionResult Login([FromBody] LoginRequest loginRequest)
    {
        // TODO: 1. Check if the credentials if they match whatever we have in the database.

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(m_authSecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, loginRequest.Email),
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
        
        return Ok(new { Token = jwtToken });
    }
}