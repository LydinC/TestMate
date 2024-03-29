﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using TestMate.API.Services;

namespace TestMate.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TestRunsController : ControllerBase
{
    private readonly TestRunsService _testRunsService;

    public TestRunsController(TestRunsService testRunsService)
    {
        _testRunsService = testRunsService;
    }

    [Authorize]
    [HttpGet("List")]
    public async Task<IActionResult> GetTestRuns(Guid RequestId)
    {
        var result = await _testRunsService.GetTestRunsByTestRequestID(RequestId);

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
    public async Task<IActionResult> GetDetails(string id)
    {
        var result = await _testRunsService.GetTestRunDetails(id);

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
    [HttpGet("{id}/Status")]
    public async Task<IActionResult> GetStatus(string id)
    {
        var result = await _testRunsService.GetStatus(id);

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
    [HttpGet("{id}/HTMLReport")]
    public async Task<IActionResult> ViewHTMLReport(string id)
    {
        var result = await _testRunsService.GetBasicHTMLReport(id);

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
    [HttpGet("{id}/NUnitReport")]
    public async Task<IActionResult> ViewNUnitReport(string id)
    {
        var result = await _testRunsService.GetNUnitReport(id);

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
    [HttpGet("{id}/HTMLReport/Download")]
    public async Task<IActionResult> DownloadHTMLReport(string id)
    {
        var result = await _testRunsService.GetFullHTMLReport(id);

        if (result.Success)
        {
            return Ok(result);
        }
        else
        {
            return BadRequest(result);
        }
    }
}