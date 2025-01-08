using MongoDB.Bson.Serialization.Attributes;

namespace ProtoActorPrototype.Persistence;

[BsonDiscriminator(nameof(PersistentSensorState))]
public class PersistentSensorState
{
    public const int
        DefaultMaxNumberOfRetainedEntries = 40,
        DefaultHistoryImageWidth = 800,
        DefaultHistoryImageHeight = 600;

    public required string Unit { get; set; }

    public required string StationName { get; set; }

    public required string ParameterName { get; set; }

    public required double Wgs84Longitude { get; set; }

    public required double Wgs84Latitude { get; set; }

    public required PersistentSensorDataEntryState[] DataEntries { get; set; }

    public required int MaxNumberOfRetainedDataEntries { get; set; } = DefaultMaxNumberOfRetainedEntries;

    public required int HistoryImageWidth { get; set; } = DefaultHistoryImageWidth;

    public required int HistoryImageHeight { get; set; } = DefaultHistoryImageHeight;
}
