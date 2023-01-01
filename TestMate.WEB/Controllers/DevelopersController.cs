
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TestMate.Common.DataTransferObjects.Developers;
using TestMate.Common.Models.Developers;
using TestMate.WEB.Models;
using TestMate.WEB.Services.Interfaces;

namespace TestMate.WEB.Controllers
{
    public class DevelopersController : Controller
    {
        private readonly IDevelopersService _service;

        public DevelopersController(IDevelopersService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public IActionResult Index()
        {
            return View();
        }

        [Route("Developers")]
        public async Task<IActionResult> Developers()
        {
            var developers = await _service.GetAllDeveloperDetails();
            return View(developers);
        }
        
        [Route("Developers/{username}")]
        public async Task<IActionResult> DeveloperDetails(string username)
        {
            var developers = await _service.GetDeveloperDetails(username);

            return View(developers);
        }

        
        [Route("Developers/Register")]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [Route("Developers/RegisterDeveloper")]
        public async Task<IActionResult> RegisterDeveloper(Developer newDeveloperDetails)
        {
            //TODO: FIX METHOD
            if (ModelState.IsValid)
            {
                var developer = await _service.RegisterDeveloper(newDeveloperDetails);
            }
            return View();
            
        }


        [HttpPost]
        [AllowAnonymous]
        [Route("Developers/Login")]
        public async Task<IActionResult> Login(DeveloperLoginDTO developerLoginDTO)
        {
            if (ModelState.IsValid)
            {
                var result = await _service.Login(developerLoginDTO);

                if (result.Success)
                {
                    TempData["message"] = "Login Successful!";

                    //Login successfull
                    HttpContext.Session.SetString("Token", result.Token);

                    Response.Cookies.Append("auth_token", result.Token, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true
                    });

                    // Redirect the user to the dashboard
                    Response.Redirect("/Developers/DeveloperDetails");
                }
                else
                {
                    //Login Failed
                    TempData["message"] = "Login Failed! Please Try Again!";
                    
                }
            }
            return View("DeveloperDetails", "Developers" );
        }

        [HttpPut]
        [Route("Developers/Edit/{username}")]
        public async Task<IActionResult> Edit(string username, Developer updatedDeveloper)
        {
            var developer = await _service.UpdateDeveloperDetails(username, updatedDeveloper);
            return View(developer);
        }
    
    
    }
}