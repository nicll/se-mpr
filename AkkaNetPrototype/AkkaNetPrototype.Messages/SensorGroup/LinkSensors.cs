using AkkaNetPrototype.Messages.Sensor;

namespace AkkaNetPrototype.Messages.SensorGroup;

public class LinkSensors : ISensorGroupMessage
{
    public string EntityId { get; set; } = String.Empty;

    public required ICollection<SensorInitializationEntry> Sensors { get; init; }
}

public record class SensorInitializationEntry(SensorId SensorId, SensorMetadata Metadata, SensorConfiguration Configuration);
