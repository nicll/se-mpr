using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrleansPrototype.GrainInterfaces;
using SharedLibraries.SensorDataParser;

namespace OrleansPrototype.ClientService.Controllers;

[Route("api/[controller]/{numericIdentifier}/{typeIdentifier}")]
[ApiController]
public class SensorController : ControllerBase
{
    private readonly ILogger<SensorController> _logger;
    private readonly IClusterClient _clusterClient;
    private readonly IDataParser _dataParser;

    public SensorController(ILogger<SensorController> logger, IClusterClient clusterClient, IDataParser dataParser)
    {
        _logger = logger;
        _clusterClient = clusterClient;
        _dataParser = dataParser;
    }

    [HttpPost]
    public async Task<IActionResult> AddData([FromRoute] long numericIdentifier, [FromRoute] string typeIdentifier, [FromQuery] double value, [FromQuery] DateTimeOffset measurementTime, [FromQuery] double quality)
    {
        var sensorGrain = _clusterClient.GetGrain<ISensorGrain>(numericIdentifier, typeIdentifier);
        await sensorGrain.AppendDataEntry(new()
        {
            Value = value,
            MeasuredAt = measurementTime,
            Quality = quality
        });
        return Ok();
    }

    [HttpGet("average")]
    public async Task<IActionResult> GetAverage([FromRoute] long numericIdentifier, [FromRoute] string typeIdentifier)
    {
        var sensorGrain = _clusterClient.GetGrain<ISensorGrain>(numericIdentifier, typeIdentifier);
        var average = await sensorGrain.GetAverage();
        return Ok(average);
    }

    [HttpGet("min")]
    public async Task<IActionResult> GetMinimum([FromRoute] long numericIdentifier, [FromRoute] string typeIdentifier)
    {
        var sensorGrain = _clusterClient.GetGrain<ISensorGrain>(numericIdentifier, typeIdentifier);
        var min = await sensorGrain.GetMinimum();
        return Ok(min);
    }

    [HttpGet("max")]
    public async Task<IActionResult> GetMaximum([FromRoute] long numericIdentifier, [FromRoute] string typeIdentifier)
    {
        var sensorGrain = _clusterClient.GetGrain<ISensorGrain>(numericIdentifier, typeIdentifier);
        var max = await sensorGrain.GetMaximum();
        return Ok(max);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistoryImage([FromRoute] long numericIdentifier, [FromRoute] string typeIdentifier)
    {
        var sensorGrain = _clusterClient.GetGrain<ISensorGrain>(numericIdentifier, typeIdentifier);
        var image = await sensorGrain.GetHistoryImage();
        return File(image.PngImage, "image/png");
    }

    [HttpPost("bulk/csv")]
    public async Task<IActionResult> AddBulkDataFromCSV()
    {
        var values = await _dataParser.LoadValues(Request.Body);
        var valuesBySensor = values.GroupBy(v => (v.numericIdentifier, v.typeIdentifier));

        foreach (var sensorDataGroup in valuesBySensor)
        {
            var sensorGrain = _clusterClient.GetGrain<ISensorGrain>(sensorDataGroup.Key.numericIdentifier, sensorDataGroup.Key.typeIdentifier);
            
            foreach (var dataEntry in sensorDataGroup)
            {
                await sensorGrain.AppendDataEntry(new()
                {
                    MeasuredAt = dataEntry.measurementTime,
                    Value = dataEntry.value,
                    Quality = 1
                });
            }
        }

        return Ok();
    }
}
