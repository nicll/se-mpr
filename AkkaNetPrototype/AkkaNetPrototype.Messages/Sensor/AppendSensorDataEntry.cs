namespace AkkaNetPrototype.Messages.Sensor;

public class AppendSensorDataEntry
{
    public required double Value { get; init; }

    public required DateTimeOffset MeasuredAt { get; init; }

    public required double Quality { get; init; }
}
