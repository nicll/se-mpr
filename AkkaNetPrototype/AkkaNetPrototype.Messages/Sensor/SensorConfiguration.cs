namespace AkkaNetPrototype.Messages.Sensor;

public class SensorConfiguration
{
    public required int MaxNumberOfRetainedDataEntries { get; init; }

    public required int HistoryImageWidth { get; init; }

    public required int HistoryImageHeight { get; init; }
}
