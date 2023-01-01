using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace TestMate.API.JWTAuthentication
{
    public class JWTTokenValidationMiddleware 
    {
        private readonly RequestDelegate _next; 
        private readonly JWTAuthenticationService _jwtAuthenticationService;
        
        public JWTTokenValidationMiddleware(RequestDelegate next, JWTAuthenticationService jwtAuthenticationService)
        {
            _next = next;
            _jwtAuthenticationService = jwtAuthenticationService;
        }

        //public async Task InvokeAsync(HttpContext context)
        //{
            // Get the JWT token from the request headers
            //var token = context.Request.Headers["Authorization"].FirstOrDefault();
            //if (token != null)
            //{
            //    // Validate the token and get the ClaimsPrincipal
            //    var claimsPrincipal = _jwtAuthenticationService.ValidateJWTToken(token);
            //    if (claimsPrincipal != null)
            //    {
            //        // Set the HttpContext.User property to the ClaimsPrincipal
            //        context.User = claimsPrincipal;
            //    }
            //    else
            //    {
            //        // If the token is invalid, return an unauthorized status code
            //        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            //        return;
            //    }
            //}
            //else
            //{
            //    // If the token is not present, return an unauthorized status code
            //    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            //    return;
            //}

            // If the token is valid, call the next middleware in the pipeline
            //await _next(context);
        //}
    }
}
