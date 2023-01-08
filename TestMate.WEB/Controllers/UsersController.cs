using Microsoft.AspNetCore.Mvc;
using TestMate.WEB.Services.Interfaces;

namespace TestMate.WEB.Controllers
{
    public class UsersController : Controller
    {

        private readonly IUsersService _service;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUsersService service, ILogger<UsersController> logger)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _logger = logger;
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