using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using TestMate.API.Services;
using TestMate.API.Services.Interfaces;
using TestMate.Common.DataTransferObjects.Devices;
using TestMate.Common.Models.Devices;

namespace TestMate.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DevicesController : ControllerBase
{
    private readonly DevicesService _devicesService;

    public DevicesController(DevicesService devicesService) =>
        _devicesService = devicesService;


    [Authorize]
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var result = await _devicesService.GetAllDevices();

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
    public async Task<IActionResult> GetDetails(string serialNumber)
    {
        var result = await _devicesService.GetDeviceBySerialNumber(serialNumber);

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
    [HttpGet("Status")]
    public async Task<IActionResult> Status(string ip)
    {
        if (ModelState.IsValid)
        {
            var result = await _devicesService.GetStatusByIP(ip);

            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return NotFound(result);
            }
        }
        else
        {
            return BadRequest(ModelState);
        }
    }

    [HttpPost("Connect")]
    public async Task<IActionResult> Connect([FromBody] DevicesConnectDTO device)
    {
        if (ModelState.IsValid)
        {
            var result = await _devicesService.ConnectDevice(device);

            if (result.Success)
            {
                return Ok(result);
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


    //[Authorize]
    [HttpPut("Update")]
    public async Task<IActionResult> Update([FromBody] Device updatedDevice)
    {
        if (ModelState.IsValid)
        {
            var result = await _devicesService.UpdateAsync(updatedDevice);

            if (result.Success)
            {
                return Ok(result);
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


   
}