namespace AkkaNetPrototype.Messages.Sensor;

public class DeleteData : ISensorMessage
{
    public required string EntityId { get; init; }
}
