using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using TestMate.API.JWTAuthentication;
using TestMate.API.Services;
using TestMate.Common.DataTransferObjects.Developers;
using TestMate.Common.Models.Developers;

namespace TestMate.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DevelopersController : ControllerBase
{
    private readonly DevelopersService _developersService;

    public DevelopersController(DevelopersService developersService)
    {
        _developersService = developersService;
    }

    //[Authorize]
    [HttpGet]
    public async Task<List<Developer>> Get() =>
            await _developersService.GetAsync();

    //[Authorize]
    [HttpGet("{username}")]
    public async Task<ActionResult<Developer>> Get(string username)
    {
        var developer = await _developersService.GetAsync(username);
        if (developer == null)
        {
            return NotFound();
        }

        return developer;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] Developer newDeveloper)
    {


        //IMPLEMENT MODEL STATE IS VALID


        //TODO: Update to get user by username ...
        var developers = await Get();



        //TODO: SWITCH THESE VALIDATIONS TO SERVICE
        if (developers.Any(i => i.Username == newDeveloper.Username))
        {
            return BadRequest($"Developer with this username already exists!");
        }

        if (developers.Any(i => i.Email == newDeveloper.Email))
        {
            return BadRequest($"Developer with this email address already exists!");
        }

        newDeveloper.IsActive = true;

        await _developersService.CreateAsync(newDeveloper);

        return CreatedAtAction(nameof(Get), new { Id = newDeveloper.Id }, newDeveloper);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] DeveloperLoginDTO developerLoginDTO)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Login service to handle the login attempt
        var result = await _developersService.Login(developerLoginDTO.Username, developerLoginDTO.Password);

        if (result.Success == true)
        {
            return Ok(result);
        }
        else
        {
            return Unauthorized(result.Message);
        }

    }


    [HttpPut("update")]
    public async Task<IActionResult> Update(Developer updatedDeveloper)
    {
        var developer = await Get(updatedDeveloper.Username);

        if (developer.Value == null)
        {
            return BadRequest("Developer '" + updatedDeveloper.Username + "' does not exist!");
        }

        updatedDeveloper.Id = developer.Value.Id;

        await _developersService.UpdateAsync(developer.Value.Username, updatedDeveloper);
        return NoContent();
    }



    [HttpPut("activate")]
    public async Task<IActionResult> Activate(string username)
    {

        var developer = await Get(username);

        if (developer.Value == null)
        {
            return NotFound();
        }

        if (developer.Value.IsActive == false)
        {
            developer.Value.IsActive = true;
            await _developersService.UpdateAsync(developer.Value.Username, developer.Value);
            return Ok("Successfully activated developer!");
        }
        else if (developer.Value.IsActive == true)
        {
            return Ok("Developer already active!");
        }
        else
        {
            return BadRequest("IsActive Property is not defined (null?)");
        }
    }

    [HttpPut("deactivate")]
    public async Task<IActionResult> Deactivate(string username)
    {

        var developer = await Get(username);

        if (developer.Value == null)
        {
            return NotFound();
        }

        if (developer.Value.IsActive == true)
        {
            developer.Value.IsActive = false;
            await _developersService.UpdateAsync(developer.Value.Username, developer.Value);
            return Ok("Successfully de-activated developer!");
        }
        else if (developer.Value.IsActive == false)
        {
            return Ok("Developer already de-active!");
        }
        else
        {
            return BadRequest("IsActive Property is not defined (null?)");
        }
    }
}
