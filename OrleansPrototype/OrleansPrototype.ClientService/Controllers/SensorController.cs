using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrleansPrototype.GrainInterfaces;

namespace OrleansPrototype.ClientService.Controllers;

[Route("api/[controller]/{numericIdentifier}/{typeIdentifier}")]
[ApiController]
public class SensorController : ControllerBase
{
    private readonly ILogger<SensorController> _logger;
    private readonly IClusterClient _clusterClient;

    public SensorController(ILogger<SensorController> logger, IClusterClient clusterClient)
    {
        _logger = logger;
        _clusterClient = clusterClient;
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
}
