namespace AkkaNetPrototype.Messages.Sensor;

public class GetStatisticResponse
{
    public required StatisticType StatisticType { get; init; }

    public required double Value { get; init; }
}
