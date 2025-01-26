namespace AkkaNetPrototype.Messages.SensorGroup;

public class SetSensorGroupMetadata : ISensorGroupMessage
{
    public required string EntityId { get; init; }
}
