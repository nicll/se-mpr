namespace AkkaNetPrototype.Messages.Sensor;

public class GetHistoryImageRequest : ISensorMessage
{
    public required string EntityId { get; init; }
}
