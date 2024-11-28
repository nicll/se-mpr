using OrleansPrototype.Models;

namespace OrleansPrototype.GrainInterfaces;

public interface ISensorGroup : IGrainWithStringKey
{
    Task SetMetaData(SensorGroupMetaData sensorGroupMetaData);
}
