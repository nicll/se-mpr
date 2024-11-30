using OrleansPrototype.Models;

namespace OrleansPrototype.GrainInterfaces;

public interface ISensorGrain : IGrainWithIntegerCompoundKey
{
    Task SetMetaData(SensorMetaData metaData);

    Task ConfigureSensor(SensorConfiguration configuration);

    Task AppendDataEntry(SensorDataEntry dataEntry);

    Task<double> GetAverage();

    Task<double> GetMin();

    Task<double> GetMax();

    Task<SensorHistoryImage> GetHistoryImage();
}
