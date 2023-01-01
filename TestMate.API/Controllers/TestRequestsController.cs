using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using TestMate.API.Services;
using TestMate.Common.DataTransferObjects.TestRequests;
using TestMate.Common.Models.TestRequests;

namespace TestMate.API.Controllers;


[ApiController]
[Route("api/[controller]")]
public class TestRequestsController : ControllerBase
{
    private readonly TestRequestsService _testRequestsService;

    public TestRequestsController(TestRequestsService testRequestsService)
    {
        _testRequestsService = testRequestsService;
    }


    [HttpGet]
    public async Task<List<TestRequest>> Get() =>
        await _testRequestsService.GetAsync();


    [HttpGet("{id}")]
    public async Task<ActionResult<TestRequest>> Get(string Id)
    {
        var testRequest = await _testRequestsService.GetAsync(Id);
        if (testRequest == null)
        {
            return NotFound();
        }

        return testRequest;
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create([FromBody] TestRequestCreateDTO testRequestCreateDTO)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        TestRequestCreateResultDTO result = await _testRequestsService.CreateAsync(testRequestCreateDTO);

        return CreatedAtAction(nameof(Get), new { Id = result.Id }, result);
    }

}
