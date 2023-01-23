using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using TestMate.API.Services;
using TestMate.Common.Models.Devices;

namespace TestMate.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DevicesController : ControllerBase
{
    private readonly DevicesService _devicesService;

    public DevicesController(DevicesService devicesService) =>
        _devicesService = devicesService;

    [HttpGet]
    public async Task<List<Device>> Get() =>
        await _devicesService.GetAsync();


    [HttpGet("{IMEI:length(15)}")]
    public async Task<ActionResult<Device>> Get(string IMEI)
    {
        var device = await _devicesService.GetAsync(IMEI);

        if (device is null)
        {
            return NotFound();
        }

        return device;
    }

    [HttpPost("Register")]
    public async Task<IActionResult> Post([FromBody] Device newDevice)
    {
        if (newDevice.IMEI.Length != 15)
        {
            return BadRequest("IMEI cannot be less than 15 digits");
        }

        if (Regex.Match(newDevice.IMEI, "^[0-9]*$").Success == false)
        {
            return BadRequest("IMEI contains non-numeric characters!");
        }

        var device = await _devicesService.GetAsync(newDevice.IMEI);
        if (device != null)
        {
            if (device.ExpirationTimestamp < DateTime.Now)
            {
                return BadRequest(String.Format("Cannot register device with IMEI {0} as it already is available", newDevice.IMEI));
            }
            else
            {
                //TODO: check if expiration timestamp should be refreshed? and send update instead of CreateAsync?
            }
        }

        //TODO: CONSIDER UTC
        newDevice.RegistrationTimestamp = DateTime.Now.ToLocalTime();
        newDevice.ExpirationTimestamp = DateTime.Now.AddHours(1).ToLocalTime();

        await _devicesService.CreateAsync(newDevice);

        return CreatedAtAction(nameof(Get), new { IMEI = newDevice.IMEI }, newDevice);
    }


    [HttpPut("{IMEI:length(15)}")]
    public async Task<IActionResult> Update(string IMEI, Device updatedDevice)
    {
        var device = await _devicesService.GetAsync(IMEI);

        if (device is null)
        {
            return NotFound();
        }

        updatedDevice.IMEI = device.IMEI;

        await _devicesService.UpdateAsync(IMEI, updatedDevice);

        return NoContent();
    }

    [HttpDelete("{IMEI:length(15)}")]
    public async Task<IActionResult> Delete(string IMEI)
    {
        var device = await _devicesService.GetAsync(IMEI);

        if (device is null)
        {
            return NotFound();
        }

        await _devicesService.RemoveAsync(IMEI);

        return NoContent();
    }
}