namespace AkkaNetPrototype.Messages.SensorGroup;

public class UnlinkSensors : ISensorGroupMessage
{
    public required string EntityId { get; init; }
}
