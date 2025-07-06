// Закомментировано для отключения авторизации
/*
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace VideoTube.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet("google-login")]
    public IActionResult GoogleLogin()
    {
        var properties = new AuthenticationProperties
        {
            RedirectUri = Url.Action("GoogleCallback"),
            Items =
            {
                { "scheme", GoogleDefaults.AuthenticationScheme },
                { "returnUrl", "/" }
            }
        };

        return Challenge(properties, GoogleDefaults.AuthenticationScheme);
    }

    [HttpGet("google-callback")]
    public async Task<IActionResult> GoogleCallback()
    {
        var result = await HttpContext.AuthenticateAsync(GoogleDefaults.AuthenticationScheme);
        
        if (!result.Succeeded)
        {
            return BadRequest("Google authentication failed");
        }

        var claims = result.Principal?.Claims.ToList() ?? new List<Claim>();
        
        // Extract user information from Google claims
        var email = claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
        var name = claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
        var picture = claims.FirstOrDefault(c => c.Type == "picture")?.Value;

        if (string.IsNullOrEmpty(email))
        {
            return BadRequest("Email not found in Google claims");
        }

        // Generate JWT token
        var token = GenerateJwtToken(email, name, picture);

        // Redirect to client with token
        var redirectUrl = $"{Request.Scheme}://{Request.Host.Host}:{Request.Host.Port}/auth-callback?token={token}";
        return Redirect(redirectUrl);
    }

    [HttpPost("validate-token")]
    public IActionResult ValidateToken([FromBody] TokenRequest request)
    {
        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "your-super-secret-key-with-at-least-32-characters");
            
            tokenHandler.ValidateToken(request.Token, new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ClockSkew = TimeSpan.Zero
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userEmail = jwtToken.Claims.First(x => x.Type == ClaimTypes.Email).Value;
            var userName = jwtToken.Claims.First(x => x.Type == ClaimTypes.Name).Value;
            var userPicture = jwtToken.Claims.FirstOrDefault(x => x.Type == "picture")?.Value;

            return Ok(new
            {
                IsValid = true,
                User = new
                {
                    Email = userEmail,
                    Name = userName,
                    Picture = userPicture
                }
            });
        }
        catch
        {
            return Ok(new { IsValid = false });
        }
    }

    private string GenerateJwtToken(string email, string? name, string? picture)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "your-super-secret-key-with-at-least-32-characters");
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Name, name ?? email),
            new Claim("picture", picture ?? ""),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}

public class TokenRequest
{
    public string Token { get; set; } = string.Empty;
}
*/ 