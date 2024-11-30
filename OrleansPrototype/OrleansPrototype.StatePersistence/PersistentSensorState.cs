namespace OrleansPrototype.StatePersistence;

[GenerateSerializer]
[Alias("OrleansPrototype.StatePersistence.PersistentSensorState")]
public class PersistentSensorState
{
    public const int
        DefaultMaxNumberOfRetainedEntries = 40,
        DefaultHistoryImageWidth = 800,
        DefaultHistoryImageHeight = 600;

    [Id(0)]
    public required string Unit { get; set; }

    [Id(1)]
    public required string StationName { get; set; }

    [Id(2)]
    public required string ParameterName { get; set; }

    [Id(3)]
    public required double Wgs84Longitude { get; set; }

    [Id(4)]
    public required float Wgs84Latitude { get; set; }

    [Id(5)]
    public required PersistentSensorDataEntryState[] DataEntries { get; set; }

    [Id(6)]
    public required int MaxNumberOfRetainedDataEntries { get; set; } = DefaultMaxNumberOfRetainedEntries;

    [Id(7)]
    public required int HistoryImageWidth { get; set; } = DefaultHistoryImageWidth;

    [Id(8)]
    public required int HistoryImageHeight { get; set; } = DefaultHistoryImageHeight;
}
