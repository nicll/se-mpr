using OrleansPrototype.Models;

namespace OrleansPrototype.GrainInterfaces;

public interface ISensorGroupGrain : IGrainWithStringKey
{
    Task SetMetaData(SensorGroupMetaData sensorGroupMetaData);

    Task LinkSensors(LinkSensorsData linkSensorsData);

    Task UnlinkSensors();
}
