using System.Text;
using System.Security.Claims;
using System.Security.Authentication;
using System.IdentityModel.Tokens.Jwt;
using LandsatReflectance.Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

using LandsatReflectance.Backend.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Org.BouncyCastle.Asn1.X509;


namespace LandsatReflectance.Backend.Controllers;

[ApiController]
[Route("")]
public class AuthenticationController : ControllerBase
{
    private readonly IUserService m_userService;
    private readonly KeysService m_keysService;
    private readonly PasswordHasher<string> m_passwordHasher;
    
    public AuthenticationController(IUserService userService, KeysService keysService)
    {
        m_userService = userService;
        m_keysService = keysService;
        m_passwordHasher = new PasswordHasher<string>();
    }


    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    [HttpPost("Login", Name = "Login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
    {
        var user = await m_userService.TryGetUser(loginRequest.Email);
        if (user is null)
        {
            return BadRequest($"Could not find the user with email \"{loginRequest.Email}\".");
        }

        var passwordVerificationResults = m_passwordHasher.VerifyHashedPassword(loginRequest.Email, user.PasswordHash, loginRequest.Password);
        if (passwordVerificationResults is PasswordVerificationResult.Failed)
        {
            return BadRequest($"An incorrect password was provided.");
        }
        
        var jwtToken = GenerateJwtToken(user);
        return Ok(new { Token = jwtToken });
    }
    
    
    public class RegisterUserRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    } 
    
    [HttpPost("Register", Name = "Register")]
    public IActionResult RegisterUser([FromBody] RegisterUserRequest registerUserRequest)
    {
        if (!IsEmailValid(registerUserRequest.Email))
        {
            return BadRequest($"The email \"{registerUserRequest.Email}\" is not valid.");
        }
        
        if (!IsPasswordValid(registerUserRequest.Password))
        {
            return BadRequest($"The password \"{registerUserRequest.Password}\" is not valid." +
                              $"\n - The password should be at least 8 characters long.");
        }
        
        var user = new User
        {
            Email = registerUserRequest.Email,
            PasswordHash = m_passwordHasher.HashPassword(registerUserRequest.Email, registerUserRequest.Password),
        };
        
        m_userService.AddUser(user);
        var jwtToken = GenerateJwtToken(user);
        
        var something = new Dictionary<string, object>
        {
            { "user", user },
            { "token", jwtToken },
        };
        
        return Ok(something);
    }

    private string GenerateJwtToken(User user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(m_keysService.AuthSecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Sub, user.Guid.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim("hashed_password",  user.PasswordHash)
        };

        var token = new JwtSecurityToken(
            issuer: "FlatEarthers",
            audience: "FlatEarthers",
            claims: claims, 
            expires: DateTime.UtcNow.AddHours(1), 
            signingCredentials: credentials
            );

        var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

        if (string.IsNullOrWhiteSpace(jwtToken))
        {
            throw new AuthenticationException("Failed to generate authentication token. Generated an null or empty string.");
        }

        return jwtToken;
    }

    // see 'https://stackoverflow.com/questions/1365407/c-sharp-code-to-validate-email-address'
    private static bool IsEmailValid(string email)
    {
        var trimmedEmail = email.Trim();

        if (trimmedEmail.EndsWith("."))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(email);
            return addr.Address == trimmedEmail;
        }
        catch
        {
            return false;
        }    
    }

    private static bool IsPasswordValid(string password)
    {
        return password.Length >= 8;
    }
}