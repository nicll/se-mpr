using Google.Protobuf.WellKnownTypes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Proto.Cluster;
using ProtoActorPrototype.Grains;
using SharedLibraries.SensorDataParser;

namespace ProtoActorPrototype.ServerService.Controllers;

[Route("api/[controller]/{numericIdentifier}/{typeIdentifier}")]
[ApiController]
public class SensorController : ControllerBase
{
    private readonly ILogger<SensorController> _logger;
    private readonly Cluster _cluster;
    private readonly IDataParser _dataParser;

    public SensorController(ILogger<SensorController> logger, Cluster cluster, IDataParser dataParser)
    {
        _logger = logger;
        _cluster = cluster;
        _dataParser = dataParser;
    }

    [HttpPost]
    public async Task<IActionResult> AddData([FromRoute] long numericIdentifier, [FromRoute] string typeIdentifier, [FromQuery] double value, [FromQuery] DateTimeOffset measurementTime, [FromQuery] double quality, CancellationToken cancellationToken)
    {
        var sensorGrain = _cluster.GetSensorGrain(numericIdentifier + '/' + typeIdentifier);
        await sensorGrain.AppendDataEntry(new()
        {
            Value = value,
            MeasuredAt = Timestamp.FromDateTimeOffset(measurementTime),
            Quality = quality
        }, cancellationToken);
        return Ok();
    }

    [HttpGet("average")]
    public async Task<IActionResult> GetAverage([FromRoute] long numericIdentifier, [FromRoute] string typeIdentifier, CancellationToken cancellationToken)
    {
        var sensorGrain = _cluster.GetSensorGrain(numericIdentifier + '/' + typeIdentifier);
        var average = await sensorGrain.GetAverage(cancellationToken);

        if (average is null)
            return NotFound();

        return Ok(average.Value);
    }

    [HttpGet("min")]
    public async Task<IActionResult> GetMinimum([FromRoute] long numericIdentifier, [FromRoute] string typeIdentifier, CancellationToken cancellationToken)
    {
        var sensorGrain = _cluster.GetSensorGrain(numericIdentifier + '/' + typeIdentifier);
        var min = await sensorGrain.GetMinimum(cancellationToken);

        if (min is null)
            return NotFound();

        return Ok(min.Value);
    }

    [HttpGet("max")]
    public async Task<IActionResult> GetMaximum([FromRoute] long numericIdentifier, [FromRoute] string typeIdentifier, CancellationToken cancellationToken)
    {
        var sensorGrain = _cluster.GetSensorGrain(numericIdentifier + '/' + typeIdentifier);
        var max = await sensorGrain.GetMaximum(cancellationToken);

        if (max is null)
            return NotFound();

        return Ok(max.Value);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistoryImage([FromRoute] long numericIdentifier, [FromRoute] string typeIdentifier, CancellationToken cancellationToken)
    {
        var sensorGrain = _cluster.GetSensorGrain(numericIdentifier + '/' + typeIdentifier);
        var image = await sensorGrain.GetHistoryImage(cancellationToken);

        if (image is null)
            return NotFound();

        return File(image.PngImage.ToArray(), "image/png");
    }

    [HttpPost("bulk/csv")]
    public async Task<IActionResult> AddBulkDataFromCSV(CancellationToken cancellationToken)
    {
        var values = await _dataParser.LoadValues(Request.Body);
        var valuesBySensor = values.GroupBy(v => (v.numericIdentifier, v.typeIdentifier));

        foreach (var sensorDataGroup in valuesBySensor)
        {
            var sensorGrain = _cluster.GetSensorGrain(sensorDataGroup.Key.numericIdentifier + '/' + sensorDataGroup.Key.typeIdentifier);

            foreach (var dataEntry in sensorDataGroup)
            {
                await sensorGrain.AppendDataEntry(new()
                {
                    MeasuredAt = Timestamp.FromDateTimeOffset(dataEntry.measurementTime),
                    Value = dataEntry.value,
                    Quality = 1
                }, cancellationToken);
            }
        }

        return Ok();
    }
}
