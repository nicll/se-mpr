namespace AkkaNetPrototype.Messages.SensorGroup;

public class GetSensorIdsRequest : ISensorGroupMessage
{
    public required string EntityId { get; init; }
}
