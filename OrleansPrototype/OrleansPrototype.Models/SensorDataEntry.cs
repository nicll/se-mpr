namespace OrleansPrototype.Models;

[GenerateSerializer]
[Alias("OrleansPrototype.Models.SensorDataEntry")]
public class SensorDataEntry
{
    /// <summary>
    /// The value measured by the sensor.
    /// </summary>
    [Id(0)]
    public required double Value { get; init; }

    /// <summary>
    /// The point in time when the value was measured.
    /// </summary>
    [Id(1)]
    public required DateTimeOffset MeasuredAt { get; init; }

    /// <summary>
    /// The quality of the measured data.
    /// If unknown, a default value of 1 is assumed.
    /// </summary>
    [Id(2)]
    public required double Quality { get; init; }
}
