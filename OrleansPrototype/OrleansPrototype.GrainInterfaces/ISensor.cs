using OrleansPrototype.Models;

namespace OrleansPrototype.GrainInterfaces;

public interface ISensor : IGrainWithIntegerCompoundKey
{
    Task SetMetaData(SensorMetaData sensorMetaData);
}
