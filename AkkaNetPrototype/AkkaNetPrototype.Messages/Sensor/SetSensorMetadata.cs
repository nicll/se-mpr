namespace AkkaNetPrototype.Messages.Sensor;

public class SetSensorMetadata
{
    public required SensorMetadata Metadata { get; init; }
}
