using Microsoft.AspNetCore.Mvc;
using TestMate.WEB.Services.Interfaces;

namespace TestMate.WEB.Controllers
{
    public class UsersController : Controller
    {

        private readonly IUsersService _service;

        public UsersController(IUsersService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public IActionResult Index()
        {
            return View();
        }

        [Route("Users")]
        public async Task<IActionResult> Users()
        {
            var users = await _service.GetAllUserDetails();
            return View(users);
        }

        [Route("Users/{username}")]
        public async Task<IActionResult> UserDetails(string username)
        {
            var users = await _service.GetUserDetails(username);
            return View(users);
        }
    }
}