using Microsoft.AspNetCore.Mvc;
using TestMate.Common.DataTransferObjects.APIResponse;
using TestMate.Common.DataTransferObjects.Developers;
using TestMate.WEB.Helpers;

namespace TestMate.WEB.Controllers
{
    public class DevelopersController : Controller
    {
        private readonly HttpClient _client;
        private readonly ILogger<DevelopersController> _logger;

        public DevelopersController(IHttpClientFactory clientFactory, ILogger<DevelopersController> logger)
        {
            _client = clientFactory.CreateClient("DevelopersClient");
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        [Route("Developers/Login")]
        public async Task<IActionResult> Login(DeveloperLoginDTO developerLoginDTO)
        {
            if (ModelState.IsValid)
            {
                var response = await _client.PostAsJsonAsync<DeveloperLoginDTO>(_client.BaseAddress + "/Login", developerLoginDTO);
                APIResponse<DeveloperLoginResultDTO> result = await response.ReadContentAsync<APIResponse<DeveloperLoginResultDTO>>();

                if (result.Success)
                {
                    string tokenString = result.Data.Token;
                    
                    HttpContext.Session.SetString("Token", tokenString);

                    //Storing JWT token in cookie
                    Response.Cookies.Append("auth_token", tokenString, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        Expires = DateTime.UtcNow.AddMinutes(15)
                    });

                    return RedirectToAction("Details", new { Username = developerLoginDTO.Username });
                }
                else
                {
                    TempData["Error"] = result.Message;
                }
            }
            return RedirectToAction("Index", "Home");
        }


        [Route("Developers")]
        public async Task<IActionResult> Developers()
        {
            var response = await _client.GetAsync("Developers");
            APIResponse<List<DeveloperDetailsDTO>> result = await response.ReadContentAsync<APIResponse<List<DeveloperDetailsDTO>>>();
            
            if (result.Success)
            {
                return View(result.Data);
            }
            else
            {
                TempData["Error"] = result.Message;
                return View();
            }
        }

        [Route("Developers/Details")]
        public async Task<IActionResult> Details()
        {

            var response = await _client.GetAsync(_client.BaseAddress + "/Details");
            APIResponse<DeveloperDetailsDTO> result = await response.ReadContentAsync<APIResponse<DeveloperDetailsDTO>>();

            if (result.Success)
            {
                var developer = result.Data;
                return View(developer);
            }
            else
            {
                TempData["Error"] = result.Message;
                return View();
            }
        }

        [Route("Developers/Register")]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [Route("Developers/Register")]
        public async Task<IActionResult> Register(DeveloperRegisterDTO developer)
        {
            var response = await _client.PostAsJsonAsync<DeveloperRegisterDTO>(_client.BaseAddress + "/Register", developer);
            APIResponse<DeveloperRegisterDTO> result = await response.ReadContentAsync<APIResponse<DeveloperRegisterDTO>>();

            if (result.Success)
            {
                TempData["Success"] = result.Message;
                return View();
            }
            else
            {
                TempData["Error"] = result.Message;
                return View();
            }
        }

        //[HttpPut]
        //[Route("Developers/Edit/{username}")]
        //public async Task<IActionResult> Edit(string username, Developer updatedDeveloper)
        //{
        //    var developer = await _client.UpdateDeveloperDetails(username, updatedDeveloper);
        //    return View(developer);
        //}


    }
}