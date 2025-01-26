namespace AkkaNetPrototype.Messages.Sensor;

public class SetSensorConfiguration : ISensorMessage
{
    public required string EntityId { get; init; }

    public required SensorConfiguration Configuration { get; init; }
}
