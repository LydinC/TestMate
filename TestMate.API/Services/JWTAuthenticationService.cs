
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using TestMate.Common.DataTransferObjects.APIResponse;
using TestMate.Common.Enums;

namespace TestMate.API.Services;

public class JWTAuthenticationService
{
    private readonly IConfiguration _configuration;

    public JWTAuthenticationService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateJWTToken(string username)
    {
        //Get JWT Authentication properties from configuration
        var secretKey = _configuration["JWTAuthentication:SecretKey"];
        var issuer = _configuration["JWTAuthentication:Issuer"];
        var audience = _configuration["JWTAuthentication:Audience"];

        var expiryTime = DateTime.UtcNow.AddMinutes(30);

        //Set the claims for the JWT
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Name, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Exp, expiryTime.ToString())
        };

        //Generate the JWT
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)), SecurityAlgorithms.HmacSha256),
            expires: expiryTime
        );

        //Return the JWT as a string to be passed to client
        return new JwtSecurityTokenHandler().WriteToken(token);
    }


    //Method not required as the JWT Bearer authentication middleware is being used and configured in startup program class
    public APIResponse<string> ValidateJWTToken(string token)
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
            var claimsPrincipal = new JwtSecurityTokenHandler().ValidateToken(token, validationParameters, out _);
            return new APIResponse<string>(Status.Ok, "Token is Valid!");
        }
        catch (SecurityTokenExpiredException)
        {
            return new APIResponse<string>(Status.Error, "Expired Token!");
        }
        catch (SecurityTokenInvalidSignatureException)
        {
            return new APIResponse<string>(Status.Error, "Invalid Signature!");
        }
        catch (SecurityTokenException)
        {
            return new APIResponse<string>(Status.Error, "Securty Token Exception!");
        }
        catch (Exception ex) { 
            return new APIResponse<string>(Status.Error, ex.Message);
        }
    }
}
