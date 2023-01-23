using System.Net.Http.Headers;

namespace TestMate.WEB.Helpers
{
    public class TokenHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TokenHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (_httpContextAccessor.HttpContext.Request.Cookies.TryGetValue("auth_token", out var accessToken))
            {
                request.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", accessToken);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }
}


