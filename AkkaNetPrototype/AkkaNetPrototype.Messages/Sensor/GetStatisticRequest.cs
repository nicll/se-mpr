namespace AkkaNetPrototype.Messages.Sensor;

public class GetStatisticRequest
{
    public required StatisticType Type { get; init; }
}
