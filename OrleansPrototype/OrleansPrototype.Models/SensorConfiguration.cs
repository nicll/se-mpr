namespace OrleansPrototype.Models;

[GenerateSerializer]
[Alias("OrleansPrototype.Models.SensorConfiguration")]
public class SensorConfiguration
{
    [Id(0)]
    public required int MaxNumberOfRetainedDataEntries { get; init; }

    [Id(1)]
    public required int HistoryImageWidth { get; init; }

    [Id(2)]
    public required int HistoryImageHeight { get; init; }
}
