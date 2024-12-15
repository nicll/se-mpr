using OrleansPrototype.Models;

namespace OrleansPrototype.GrainInterfaces;

public interface ISensorGrain : IGrainWithIntegerCompoundKey
{
    Task SetMetaData(SensorMetaData metaData);

    Task ConfigureSensor(SensorConfiguration configuration);

    Task AppendDataEntry(SensorDataEntry dataEntry);

    Task<double> GetAverage();

    Task<double> GetMinimum();

    Task<double> GetMaximum();

    Task<SensorHistoryImage> GetHistoryImage();

    Task DeleteData();
}
