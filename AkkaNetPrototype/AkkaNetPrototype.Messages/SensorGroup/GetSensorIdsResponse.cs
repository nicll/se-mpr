namespace AkkaNetPrototype.Messages.SensorGroup;

public class GetSensorIdsResponse
{
    public required ICollection<SensorId> Ids { get; init; }
}

public record class SensorId(long NumericIdentifier, string TypeIdentifier);
