using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using MongoDB.Driver;
using TestMate.API.Services;
using TestMate.API.Services.Interfaces;
using TestMate.Common.DataTransferObjects.Developers;
using TestMate.Common.DataTransferObjects.Users;
using TestMate.Common.Models.Users;

namespace TestMate.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly UsersService _usersService;

    public UsersController(UsersService usersService) =>
        _usersService = usersService;



    //[Authorize]
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result = await _usersService.GetAllUsers();

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
        var result = await _usersService.GetUser(username);

        if (result.Success)
        {
            return Ok(result);
        }
        else
        {
            return NotFound(result);
        }
    }


    [HttpPost("Login")]
    public async Task<IActionResult> Login([FromBody] UserLoginDTO userLoginDTO)
    {
        if (ModelState.IsValid)
        {
            var result = await _usersService.Login(userLoginDTO);
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

    [AllowAnonymous]
    [HttpPost("Register")]
    public async Task<IActionResult> Register([FromBody] UserRegisterDTO user)
    {
        if (ModelState.IsValid)
        {
            var result = await _usersService.Register(user);

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


    //[HttpPut("Update")]
    //public async Task<IActionResult> Update(User updatedUser)
    //{
    //    var user = await GetDetails(updatedUser.Username);

    //    if (user.Value == null)
    //    {
    //        return BadRequest("User '" + updatedUser.Username + "' does not exist!");
    //    }

    //    updatedUser.Id = user.Value.Id;

    //    await _usersService.UpdateAsync(user.Value.Username, updatedUser);
    //    return NoContent();
    //}



    //[HttpPut("Activate")]
    //public async Task<IActionResult> Activate(string username)
    //{

    //    var user = await Get(username);

    //    if (user.Value == null)
    //    {
    //        return NotFound();
    //    }

    //    if (user.Value.IsActive == false)
    //    {
    //        user.Value.IsActive = true;
    //        await _usersService.UpdateAsync(user.Value.Username, user.Value);
    //        return Ok("Successfully activated user!");
    //    }
    //    else if (user.Value.IsActive == true)
    //    {
    //        return Ok("User already active!");
    //    }
    //    else
    //    {
    //        return BadRequest("IsActive Property is not defined (null?)");
    //    }
    //}

    //[HttpPut("Deactivate")]
    //public async Task<IActionResult> Deactivate(string username)
    //{

    //    var user = await Get(username);

    //    if (user.Value == null)
    //    {
    //        return NotFound();
    //    }

    //    if (user.Value.IsActive == true)
    //    {
    //        user.Value.IsActive = false;
    //        await _usersService.UpdateAsync(user.Value.Username, user.Value);
    //        return Ok("Successfully de-activated user!");
    //    }
    //    else if (user.Value.IsActive == false)
    //    {
    //        return Ok("User already de-active!");
    //    }
    //    else
    //    {
    //        return BadRequest("IsActive Property is not defined (null?)");
    //    }
    //}
}
