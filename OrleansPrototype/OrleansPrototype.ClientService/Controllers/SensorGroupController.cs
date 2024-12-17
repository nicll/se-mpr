using Microsoft.AspNetCore.Mvc;
using OrleansPrototype.GrainInterfaces;
using OrleansPrototype.Models;

namespace OrleansPrototype.ClientService.Controllers;

[Route("api/[controller]/{groupIdentifier}")]
[ApiController]
public class SensorGroupController : ControllerBase
{
    private readonly ILogger<SensorGroupController> _logger;
    private readonly IClusterClient _clusterClient;

    public SensorGroupController(ILogger<SensorGroupController> logger, IClusterClient clusterClient)
    {
        _logger = logger;
        _clusterClient = clusterClient;
    }

    [HttpPost]
    public async Task<IActionResult> LinkSensors([FromRoute] string groupIdentifier, [FromBody] LinkSensorsData data)
    {
        var sensorGroupGrain = _clusterClient.GetGrain<ISensorGroupGrain>(groupIdentifier);
        await sensorGroupGrain.LinkSensors(data);
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> ListSensors([FromRoute] string groupIdentifier)
    {
        try
        {
            var sensorGroupGrain = _clusterClient.GetGrain<ISensorGroupGrain>(groupIdentifier);
            var sensorIds = await sensorGroupGrain.ListSensors();

            if (sensorIds is null)
                return NotFound();

            return Ok(sensorIds.Ids);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to list sensors of group {groupIdentifier}.", groupIdentifier);
            return Problem(e.ToString());
        }
    }

    [HttpDelete]
    public async Task<IActionResult> UnlinkSensors([FromRoute] string groupIdentifier)
    {
        try
        {
            var sensorGroupGrain = _clusterClient.GetGrain<ISensorGroupGrain>(groupIdentifier);
            await sensorGroupGrain.UnlinkSensors();
            return Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to unlink sensors of group {groupIdentifier}.", groupIdentifier);
            return Problem(e.ToString());
        }
    }
}
