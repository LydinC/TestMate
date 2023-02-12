using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using TestMate.API.Services;
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
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result = await _testRequestsService.GetTestRequests();
        
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
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string Id)
    {
        var result = await _testRequestsService.GetById(Id);

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
