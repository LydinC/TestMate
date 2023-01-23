//using Microsoft.AspNetCore.Mvc.Filters;

//namespace TestMate.API.JWTAuthentication
//{
//    public class SkipMiddlewareAttribute : Attribute, IAsyncActionFilter
//    {
//        private readonly Type _middlewareType;
//        private readonly RequestDelegate _next;

//        public SkipMiddlewareAttribute(Type middlewareType, RequestDelegate next)
//        {
//            _middlewareType = middlewareType;
//            _next = next;
//        }

//        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
//        {
//            var httpContext = context.HttpContext;

//            // Check if the middleware has already been executed
//            if (httpContext.Items.ContainsKey(_middlewareType))
//            {
//                // The middleware has already been executed, so skip it
//                await next();
//            }
//            else
//            {
//                // The middleware has not yet been executed, so execute it
//                httpContext.Items[_middlewareType] = true;
//                await ((IMiddleware)httpContext.RequestServices.GetRequiredService(_middlewareType)).InvokeAsync(httpContext, _next);
//            }
//        }
//    }
//}
