using MongoDB.Bson.Serialization.Attributes;

namespace ProtoActorPrototype.Persistence;

[BsonDiscriminator(nameof(PersistentSensorGroupState))]
public class PersistentSensorGroupState
{
    [BsonElement]
    public required (long numericIdentifier, string typeIdentifier)[] LinkedSensors { get; set; }
}
