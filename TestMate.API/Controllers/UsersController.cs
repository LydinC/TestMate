using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using TestMate.API.Services;
using TestMate.Common.Models.Users;

namespace TestMate.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UsersService _usersService;

    public UsersController(UsersService usersService) =>
        _usersService = usersService;

    [HttpGet]
    public async Task<List<User>> Get() =>
    await _usersService.GetAsync();


    [HttpGet("{username}")]
    public async Task<ActionResult<User>> Get(string username)
    {
        var user = await _usersService.GetAsync(username);
        if (user == null)
        {
            return NotFound();
        }

        return user;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] User newUser)
    {

        var users = await Get();

        if (users.Any(i => i.Username == newUser.Username))
        {
            return BadRequest($"User with this username already exists!");
        }
        if (users.Any(i => i.Email == newUser.Email))
        {
            return BadRequest($"Developer with this email address already exists!");
        }

        newUser.IsActive = true;

        await _usersService.CreateAsync(newUser);

        return CreatedAtAction(nameof(Get), new { Id = newUser.Id }, newUser.Username);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(string username, string password)
    {
        var user = await _usersService.GetAsync(username);

        if (user == null)
        {
            return BadRequest($"User {username} does not exist!");
        }

        if (user.Password != password)
        {
            return BadRequest("Invalid Password!");
        }

        return Ok("Successfully logged in!");
    }


    [HttpPut("update")]
    public async Task<IActionResult> Update(User updatedUser)
    {
        var user = await Get(updatedUser.Username);

        if (user.Value == null)
        {
            return BadRequest("User '" + updatedUser.Username + "' does not exist!");
        }

        updatedUser.Id = user.Value.Id;

        await _usersService.UpdateAsync(user.Value.Username, updatedUser);
        return NoContent();
    }



    [HttpPut("activate")]
    public async Task<IActionResult> Activate(string username)
    {

        var user = await Get(username);

        if (user.Value == null)
        {
            return NotFound();
        }

        if (user.Value.IsActive == false)
        {
            user.Value.IsActive = true;
            await _usersService.UpdateAsync(user.Value.Username, user.Value);
            return Ok("Successfully activated user!");
        }
        else if (user.Value.IsActive == true)
        {
            return Ok("User already active!");
        }
        else
        {
            return BadRequest("IsActive Property is not defined (null?)");
        }
    }

    [HttpPut("deactivate")]
    public async Task<IActionResult> Deactivate(string username)
    {

        var user = await Get(username);

        if (user.Value == null)
        {
            return NotFound();
        }

        if (user.Value.IsActive == true)
        {
            user.Value.IsActive = false;
            await _usersService.UpdateAsync(user.Value.Username, user.Value);
            return Ok("Successfully de-activated user!");
        }
        else if (user.Value.IsActive == false)
        {
            return Ok("User already de-active!");
        }
        else
        {
            return BadRequest("IsActive Property is not defined (null?)");
        }
    }
}
