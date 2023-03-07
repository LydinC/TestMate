using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using TestMate.WEB.Helpers;
using TestMate.WEB.Models;

namespace TestMate.WEB.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return RedirectToAction("Login", "Home");
        }
        public IActionResult Login()
        {
            string? auth_token;
            Request.Cookies.TryGetValue("auth_token", out auth_token);
            if (auth_token != null)
            {
                return RedirectToAction("Details", "Developers");
            }
            else 
            {
                return View();
            }
        }

        public IActionResult LogOff()
        {
            HttpContext.Session.Clear();
            Response.Cookies.Delete("auth_token");
            return RedirectToAction("Login");
        }

        public IActionResult AboutUs()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}