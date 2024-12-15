using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrleansPrototype.GrainInterfaces;

namespace OrleansPrototype.ClientService.Controllers;

[Route("api/[controller]/[action]")]
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
    public async Task<IActionResult> AddData(long numericIdentifier, string typeIdentifier, double value, DateTimeOffset measurementTime, double quality)
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

    public async Task<IActionResult> GetAverage(long numericIdentifier, string typeIdentifier)
    {
        var sensorGrain = _clusterClient.GetGrain<ISensorGrain>(numericIdentifier, typeIdentifier);
        var average = await sensorGrain.GetAverage();
        return Ok(average);
    }

    public async Task<IActionResult> GetMinimum(long numericIdentifier, string typeIdentifier)
    {
        var sensorGrain = _clusterClient.GetGrain<ISensorGrain>(numericIdentifier, typeIdentifier);
        var min = await sensorGrain.GetMinimum();
        return Ok(min);
    }

    public async Task<IActionResult> GetMaximum(long numericIdentifier, string typeIdentifier)
    {
        var sensorGrain = _clusterClient.GetGrain<ISensorGrain>(numericIdentifier, typeIdentifier);
        var max = await sensorGrain.GetMaximum();
        return Ok(max);
    }

    public async Task<IActionResult> GetHistoryImage(long numericIdentifier, string typeIdentifier)
    {
        var sensorGrain = _clusterClient.GetGrain<ISensorGrain>(numericIdentifier, typeIdentifier);
        var image = await sensorGrain.GetHistoryImage();
        return File(image.PngImage, "image/png");
    }
}
