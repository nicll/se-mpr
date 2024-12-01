namespace OrleansPrototype.StatePersistence;

[GenerateSerializer]
[Alias("OrleansPrototype.StatePersistence.PersistentSensorGroupState")]
public class PersistentSensorGroupState
{
    [Id(0)]
    public required (long numericIdentifier, string typeIdentifier)[] LinkedSensors { get; set; }
}
