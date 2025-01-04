using Google.Protobuf;
using Microsoft.AspNetCore.Mvc;
using Proto.Cluster;
using ProtoActorPrototype.Grains;

namespace ProtoActorPrototype.ServerService.Controllers;

[Route("api/[controller]/{groupIdentifier}")]
[ApiController]
public class SensorGroupController : ControllerBase
{
    private readonly ILogger<SensorGroupController> _logger;
    private readonly Cluster _cluster;

    public SensorGroupController(ILogger<SensorGroupController> logger, Cluster cluster)
    {
        _logger = logger;
        _cluster = cluster;
    }

    [HttpPost]
    public async Task<IActionResult> LinkSensors([FromRoute] string groupIdentifier, CancellationToken cancellationToken)
    {
        using var streamReader = new StreamReader(Request.Body);
        var message = JsonParser.Default.Parse<LinkSensorsData>(streamReader);
        var sensorGroupGrain = _cluster.GetSensorGroupGrain(groupIdentifier);
        await sensorGroupGrain.LinkSensors(message, cancellationToken);
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> ListSensors([FromRoute] string groupIdentifier, CancellationToken cancellationToken)
    {
        try
        {
            var sensorGroupGrain = _cluster.GetSensorGroupGrain(groupIdentifier);
            var sensorIds = await sensorGroupGrain.ListSensors(cancellationToken);

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
    public async Task<IActionResult> UnlinkSensors([FromRoute] string groupIdentifier, CancellationToken cancellationToken)
    {
        try
        {
            var sensorGroupGrain = _cluster.GetSensorGroupGrain(groupIdentifier);
            await sensorGroupGrain.UnlinkSensors(cancellationToken);
            return Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to unlink sensors of group {groupIdentifier}.", groupIdentifier);
            return Problem(e.ToString());
        }
    }
}
