using Microsoft.AspNetCore.Mvc;
using TestMate.API.Services;

namespace TestMate.API.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    public class AuthenticationController : ControllerBase
    {
        private readonly JWTAuthenticationService _jwtAuthenticationService;

        public AuthenticationController(JWTAuthenticationService jwtAuthenticationService)
        {
            _jwtAuthenticationService = jwtAuthenticationService;
        }

        [HttpGet]
        public IActionResult Authenticate(string token)
        {
            var result = _jwtAuthenticationService.ValidateJWTToken(token);

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return Unauthorized(result);
            }

        }

    }
}
