//using System;
//using System.Net.Http;
//using System.Threading.Tasks;
//using Microsoft.AspNetCore.Http;

//namespace TestMate.WEB.Helpers
//{
//    public class AuthenticationHelper
//    {
//        private readonly IHttpContextAccessor _httpContextAccessor;
//        private readonly HttpClient _client;
//        private readonly ILogger<AuthenticationHelper> _logger;
        
//        public AuthenticationHelper(IHttpClientFactory clientFactory,ILogger<AuthenticationHelper> logger, IHttpContextAccessor httpContextAccessor)
//        {
//            _client = clientFactory.CreateClient("AuthenticationClient");
//            _logger = logger;
//            _httpContextAccessor = httpContextAccessor;
//        }

//        //The below method might be used for instances where we want to valid the client token on server side
//        public async Task<bool> IsUserAuthenticated()
//        {
//            var httpContext = _httpContextAccessor.HttpContext;
//            if (httpContext == null) { return false; }

//            var request = httpContext.Request;
//            var token = request.Cookies["auth_token"];

//            if (token == null || token == "") { 
//                return false;
//            }


//            //Validate the JWT Token against server side using API call
//            try
//            {
//                var response = await _client.PostAsync(_client.BaseAddress, new StringContent(token));
//                if (response.IsSuccessStatusCode)
//                {
//                    return true;
//                }
//                else
//                {
//                    return false;
//                }
//            } catch(Exception ex) 
//            {
//                _logger.LogError(ex.Message);
//                return false;
//            }
            
//        }
    
    
//        public bool IsAuthenticated() {

//            //if(_httpContextAccessor.HttpContext.Session.)

//            return false;
//        }
    
//    }
//}
