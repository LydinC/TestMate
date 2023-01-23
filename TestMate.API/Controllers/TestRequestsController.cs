using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

    [HttpPost("Create")]
    public async Task<IActionResult> Create([FromBody] TestRequestCreateDTO testRequestCreateDTO)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _testRequestsService.CreateAsync(testRequestCreateDTO);
        if (result.Success)
        {
            return CreatedAtAction(nameof(Get), result);
        }
        else {
            return BadRequest(result);
        }
    }

}
