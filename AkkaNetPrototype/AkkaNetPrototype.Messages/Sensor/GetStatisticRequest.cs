namespace AkkaNetPrototype.Messages.Sensor;

public class GetStatisticRequest : ISensorMessage
{
    public required string EntityId { get; init; }

    public required StatisticType Type { get; init; }
}
