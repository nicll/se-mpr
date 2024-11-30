namespace OrleansPrototype.StatePersistence;

[GenerateSerializer]
[Alias("OrleansPrototype.StatePersistence.PersistentSensorDataEntryState")]
public class PersistentSensorDataEntryState
{
    [Id(0)]
    public required double Value { get; init; }

    [Id(1)]
    public required DateTimeOffset MeasuredAt { get; init; }

    [Id(2)]
    public required double Quality { get; init; }
}
