using Akka.Actor;
using Akka.Hosting;
using AkkaNetPrototype.Messages.SensorGroup;
using Microsoft.AspNetCore.Mvc;

namespace AkkaNetPrototype.ClientService.Controllers;

[Route("api/[controller]/{groupIdentifier}")]
[ApiController]
public class SensorGroupController : ControllerBase
{
    private readonly ILogger<SensorGroupController> _logger;
    private readonly ActorRegistry _actorRegistry;

    public SensorGroupController(ILogger<SensorGroupController> logger, ActorRegistry actorRegistry)
    {
        _logger = logger;
        _actorRegistry = actorRegistry;
    }

    [HttpPost]
    public async Task<IActionResult> LinkSensors([FromRoute] string groupIdentifier, [FromBody] LinkSensors data)
    {
        var sensorGroupActorShard = await _actorRegistry.GetAsync<ISensorGroupMessage>();
        data.EntityId = groupIdentifier;
        sensorGroupActorShard.Tell(data);
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> ListSensors([FromRoute] string groupIdentifier)
    {
        try
        {
            var sensorGroupActorShard = await _actorRegistry.GetAsync<ISensorGroupMessage>();
            var sensorIds = await sensorGroupActorShard.Ask<GetSensorIdsResponse>(new GetSensorIdsRequest { EntityId = groupIdentifier });

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
            var sensorGroupActorShard = await _actorRegistry.GetAsync<ISensorGroupMessage>();
            sensorGroupActorShard.Tell(new UnlinkSensors { EntityId = groupIdentifier });
            return Ok();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to unlink sensors of group {groupIdentifier}.", groupIdentifier);
            return Problem(e.ToString());
        }
    }
}
