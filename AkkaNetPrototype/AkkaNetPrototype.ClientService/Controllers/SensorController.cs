using Akka.Actor;
using Akka.Hosting;
using AkkaNetPrototype.Messages.Sensor;
using Microsoft.AspNetCore.Mvc;
using SharedLibraries.SensorDataParser;

namespace AkkaNetPrototype.ClientService.Controllers;

[Route("api/[controller]/{numericIdentifier}/{typeIdentifier}")]
[ApiController]
public class SensorController : ControllerBase
{
    private readonly ILogger<SensorController> _logger;
    private readonly IActorRegistry _actorRegistry;
    private readonly IDataParser _dataParser;

    public SensorController(ILogger<SensorController> logger, IActorRegistry actorRegistry, IDataParser dataParser)
    {
        _logger = logger;
        _actorRegistry = actorRegistry;
        _dataParser = dataParser;
    }

    [HttpPost]
    public async Task<IActionResult> AddData([FromRoute] long numericIdentifier, [FromRoute] string typeIdentifier, [FromQuery] double value, [FromQuery] DateTimeOffset measurementTime, [FromQuery] double quality)
    {
        var sensorActorShard = await _actorRegistry.GetAsync<ISensorMessage>();
        sensorActorShard.Tell(new AppendSensorDataEntry
        {
            EntityId = numericIdentifier + '#' + typeIdentifier,
            Value = value,
            MeasuredAt = measurementTime,
            Quality = quality
        });
        return Ok();
    }

    [HttpGet("average")]
    public async Task<IActionResult> GetAverage([FromRoute] long numericIdentifier, [FromRoute] string typeIdentifier)
    {
        var sensorActorShard = await _actorRegistry.GetAsync<ISensorMessage>();
        var average = await sensorActorShard.Ask<GetStatisticResponse>(new GetStatisticRequest
        {
            EntityId = numericIdentifier + '#' + typeIdentifier,
            Type = StatisticType.Average
        });
        return Ok(average);
    }

    [HttpGet("min")]
    public async Task<IActionResult> GetMinimum([FromRoute] long numericIdentifier, [FromRoute] string typeIdentifier)
    {
        var sensorActorShard = await _actorRegistry.GetAsync<ISensorMessage>();
        var min = await sensorActorShard.Ask<GetStatisticResponse>(new GetStatisticRequest
        {
            EntityId = numericIdentifier + '#' + typeIdentifier,
            Type = StatisticType.Min
        });
        return Ok(min);
    }

    [HttpGet("max")]
    public async Task<IActionResult> GetMaximum([FromRoute] long numericIdentifier, [FromRoute] string typeIdentifier)
    {
        var sensorActorShard = await _actorRegistry.GetAsync<ISensorMessage>();
        var max = await sensorActorShard.Ask<GetStatisticResponse>(new GetStatisticRequest
        {
            EntityId = numericIdentifier + '#' + typeIdentifier,
            Type = StatisticType.Max
        });
        return Ok(max);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistoryImage([FromRoute] long numericIdentifier, [FromRoute] string typeIdentifier)
    {
        var sensorActorShard = await _actorRegistry.GetAsync<ISensorMessage>();
        var image = await sensorActorShard.Ask<GetHistoryImageResponse>(new GetHistoryImageRequest
        {
            EntityId = numericIdentifier + '#' + typeIdentifier,
        });
        return File(image.PngImage, "image/png");
    }

    [HttpPost("bulk/csv")]
    public async Task<IActionResult> AddBulkDataFromCSV()
    {
        var values = await _dataParser.LoadValues(Request.Body);
        var valuesBySensor = values.GroupBy(v => (v.numericIdentifier, v.typeIdentifier));

        foreach (var sensorDataGroup in valuesBySensor)
        {
            var sensorActorShard = await _actorRegistry.GetAsync<ISensorMessage>();

            foreach (var dataEntry in sensorDataGroup)
            {
                sensorActorShard.Tell(new AppendSensorDataEntry
                {
                    EntityId = sensorDataGroup.Key.numericIdentifier + '#' + sensorDataGroup.Key.typeIdentifier,
                    MeasuredAt = dataEntry.measurementTime,
                    Value = dataEntry.value,
                    Quality = 1
                });
            }
        }

        return Ok();
    }
}
