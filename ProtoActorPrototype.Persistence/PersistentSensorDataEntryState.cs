using MongoDB.Bson.Serialization.Attributes;

namespace ProtoActorPrototype.Persistence;

[BsonDiscriminator(nameof(PersistentSensorDataEntryState))]
public class PersistentSensorDataEntryState
{
    public required double Value { get; init; }

    public required DateTimeOffset MeasuredAt { get; init; }

    public required double Quality { get; init; }
}
