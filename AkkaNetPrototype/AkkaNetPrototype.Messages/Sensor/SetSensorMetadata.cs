namespace AkkaNetPrototype.Messages.Sensor;

public class SetSensorMetadata : ISensorMessage
{
    public required string EntityId { get; init; }

    public required SensorMetadata Metadata { get; init; }
}
