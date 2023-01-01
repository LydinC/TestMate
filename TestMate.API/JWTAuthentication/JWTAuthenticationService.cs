using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace TestMate.API.JWTAuthentication;

public class JWTAuthenticationService
{
    private readonly IConfiguration _configuration;

    public JWTAuthenticationService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateJWTToken(string username)
    {
        // Get JWT Authentication properties from configuration
        var secretKey = _configuration["JWTAuthentication:SecretKey"];
        var issuer = _configuration["JWTAuthentication:Issuer"];
        var audience = _configuration["JWTAuthentication:Audience"];

        // Set the expiry time for the JWT
        var expiryTime = DateTime.UtcNow.AddMinutes(30);

        // Set the claims for the JWT
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Exp, expiryTime.ToString())
        };

        // Generate the JWT
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)), SecurityAlgorithms.HmacSha256),
            expires: expiryTime
        );

        // Return the JWT as a string
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public ClaimsPrincipal ValidateJWTToken(string token)
    {

        // Get JWT Authentication properties from configuration
        var secretKey = _configuration["JWTAuthentication:SecretKey"];
        var issuer = _configuration["JWTAuthentication:Issuer"];
        var audience = _configuration["JWTAuthentication:Audience"];

        // Set the validation parameters for the JWT
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ValidateIssuerSigningKey = true
        };

        try
        {
            // Validate the JWT
            var claimsPrincipal = new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out _);

            return claimsPrincipal;
        }
        catch (SecurityTokenExpiredException)
        {
            // The JWT has expired
            return null;
        }
        catch (SecurityTokenInvalidSignatureException)
        {
            // The JWT has an invalid signature
            return null;
        }
        catch (SecurityTokenException)
        {
            // The JWT is invalid for other reasons
            return null;
        }
    }
}
