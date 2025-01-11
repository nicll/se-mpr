namespace AkkaNetPrototype.Messages.Sensor;

public class SetSensorConfiguration
{
    public required SensorConfiguration Configuration { get; init; }
}
