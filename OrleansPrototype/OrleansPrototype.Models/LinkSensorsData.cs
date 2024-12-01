namespace OrleansPrototype.Models;

[GenerateSerializer]
[Alias("OrleansPrototype.Models.CreateSensorsData")]
public class LinkSensorsData
{
    [Id(0)]
    public required ICollection<SensorInitializationEntry> Sensors { get; init; }
}

[GenerateSerializer]
[Alias("OrleansPrototype.Models.SensorInitializationEntry")]
public record class SensorInitializationEntry(SensorId SensorId, SensorMetaData MetaData, SensorConfiguration Configuration);
