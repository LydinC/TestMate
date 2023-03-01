using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using TestMate.API.Services;
using TestMate.API.Services.Interfaces;
using TestMate.Common.DataTransferObjects.TestRequests;

namespace TestMate.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TestRequestsController : ControllerBase
{
    private readonly TestRequestsService _testRequestsService;

    public TestRequestsController(TestRequestsService testRequestsService)
    {
        _testRequestsService = testRequestsService;
    }

    [Authorize]
    [HttpGet("All")]
    public async Task<IActionResult> GetAll()
    {
        var result = await _testRequestsService.GetAllTestRequests();
        
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
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        string username = User.FindFirst(JwtRegisteredClaimNames.Name).Value ?? throw new ArgumentNullException();
        var result = await _testRequestsService.GetTestRequests(username);

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
    public async Task<IActionResult> Details(Guid RequestId)
    {
        var result = await _testRequestsService.GetDetails(RequestId);

        if (result.Success)
        {
            return Ok(result);
        }
        else
        {
            return NotFound(result);
        }
    }

    [Authorize]
    [HttpGet("{id}/Status")]
    public async Task<IActionResult> Status(Guid RequestId)
    {
        var result = await _testRequestsService.GetStatus(RequestId);

        if (result.Success)
        {
            return Ok(result);
        }
        else
        {
            return NotFound(result);
        }
    }


    [Authorize]
    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromBody] TestRequestCreateDTO testRequestCreateDTO)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        string requestorUsername = User.FindFirst(JwtRegisteredClaimNames.Name).Value ?? throw new ArgumentNullException();
        var result = await _testRequestsService.CreateAsync(requestorUsername, testRequestCreateDTO);
        if (result.Success)
        {
            return CreatedAtAction(nameof(Get), result);
        }
        else {
            return BadRequest(result);
        }
    }

}
