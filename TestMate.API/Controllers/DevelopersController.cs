using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using TestMate.API.Services;
using TestMate.Common.DataTransferObjects.Developers;

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

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result = await _developersService.GetAllDevelopers();

        if (result.Success)
        {
            return Ok(result);
        }
        else
        {
            return BadRequest(result);
        }
    }

    [Authorize]
    [HttpGet("Details")]
    public async Task<IActionResult> GetDetails()
    {
        string username = User.FindFirst(JwtRegisteredClaimNames.Name).Value ?? throw new ArgumentNullException();
        var result = await _developersService.GetDeveloper(username);

        if (result.Success)
        {
            return Ok(result);
        }
        else
        {
            return NotFound(result);
        }
    }

    [AllowAnonymous]
    [HttpPost("Register")]
    public async Task<IActionResult> Register([FromBody] DeveloperRegisterDTO developer)
    {
        if (ModelState.IsValid)
        {
            var result = await _developersService.Register(developer);

            if (result.Success)
            {
                return CreatedAtAction(nameof(Get), result);
            }
            else
            {
                return BadRequest(result);
            }
        }
        else
        {
            return BadRequest(ModelState);
        }
    }

    [AllowAnonymous]
    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] DeveloperLoginDTO developerLoginDTO)
    {
        if (ModelState.IsValid)
        {
            var result = await _developersService.Login(developerLoginDTO);
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return Unauthorized(result);
            }
        }
        else
        {
            return BadRequest(ModelState);
        }

    }


    //[HttpPut("update")]
    //public async Task<IActionResult> Update(Developer updatedDeveloper)
    //{
    //    var developer = await Get(updatedDeveloper.Username);

    //    if (developer.Value == null)
    //    {
    //        return BadRequest("Developer '" + updatedDeveloper.Username + "' does not exist!");
    //    }

    //    updatedDeveloper.Id = developer.Value.Id;

    //    await _developersService.UpdateAsync(developer.Value.Username, updatedDeveloper);
    //    return NoContent();
    //}



    //[HttpPut("activate")]
    //public async Task<IActionResult> Activate(string username)
    //{

    //    var developer = await Get(username);

    //    if (developer.Value == null)
    //    {
    //        return NotFound();
    //    }

    //    if (developer.Value.IsActive == false)
    //    {
    //        developer.Value.IsActive = true;
    //        await _developersService.UpdateAsync(developer.Value.Username, developer.Value);
    //        return Ok("Successfully activated developer!");
    //    }
    //    else if (developer.Value.IsActive == true)
    //    {
    //        return Ok("Developer already active!");
    //    }
    //    else
    //    {
    //        return BadRequest("IsActive Property is not defined (null?)");
    //    }
    //}

    //[HttpPut("deactivate")]
    //public async Task<IActionResult> Deactivate(string username)
    //{

    //    var developer = await Get(username);

    //    if (developer.Value == null)
    //    {
    //        return NotFound();
    //    }

    //    if (developer.Value.IsActive == true)
    //    {
    //        developer.Value.IsActive = false;
    //        await _developersService.UpdateAsync(developer.Value.Username, developer.Value);
    //        return Ok("Successfully de-activated developer!");
    //    }
    //    else if (developer.Value.IsActive == false)
    //    {
    //        return Ok("Developer already de-active!");
    //    }
    //    else
    //    {
    //        return BadRequest("IsActive Property is not defined (null?)");
    //    }
    //}
}
