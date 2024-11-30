using Microsoft.Extensions.Logging;
using OrleansPrototype.GrainInterfaces;
using OrleansPrototype.Models;
using OrleansPrototype.StatePersistence;

namespace OrleansPrototype.GrainImplementations;

public class SensorGrain : Grain, ISensorGrain
{
    private readonly IPersistentState<PersistentSensorState> _persistedState;
    private readonly ILogger<SensorGrain> _logger;

    public SensorGrain(
        [PersistentState(nameof(PersistentSensorState))] IPersistentState<PersistentSensorState> persistedState,
        ILogger<SensorGrain> logger)
    {
        _persistedState = persistedState;
        _logger = logger;
    }

    public async Task SetMetaData(SensorMetaData metaData)
    {
        _persistedState.State.Unit = metaData.Unit;
        _persistedState.State.StationName = metaData.StationName;
        _persistedState.State.ParameterName = metaData.ParameterName;
        _persistedState.State.Wgs84Longitude = metaData.Wgs84Longitude;
        _persistedState.State.Wgs84Latitude = metaData.Wgs84Latitude;
        await _persistedState.WriteStateAsync();
    }

    public async Task ConfigureSensor(SensorConfiguration configuration)
    {
        _persistedState.State.MaxNumberOfRetainedDataEntries = configuration.MaxNumberOfRetainedDataEntries;
        await _persistedState.WriteStateAsync();
    }

    public async Task AppendDataEntry(SensorDataEntry dataEntry)
    {
        // prepare the data we're handling
        var maxNumberOfRetainedEntries = _persistedState.State.MaxNumberOfRetainedDataEntries;
        var persistentDataEntry = new PersistentSensorDataEntryState
        {
            Value = dataEntry.Value,
            MeasuredAt = dataEntry.MeasuredAt,
            Quality = dataEntry.Quality
        };

        var newEntries = _persistedState.State.DataEntries.Append(persistentDataEntry);

        // remove the first few entries if we have too many
        if (newEntries.Count() > maxNumberOfRetainedEntries)
            newEntries = newEntries.Skip(newEntries.Count() - maxNumberOfRetainedEntries);

        // swap out data entries and save
        _persistedState.State.DataEntries = newEntries.ToArray();
        await _persistedState.WriteStateAsync();
    }

    public Task<double> GetAverage()
    {
        if (_persistedState.State.DataEntries is null or { Length: < 1 })
            throw new InvalidOperationException("Cannot calculate average when no data entries exist.");

        return Task.FromResult(_persistedState.State.DataEntries
            .Select(e => e.Value)
            .Average());
    }

    public Task<double> GetMin()
    {
        if (_persistedState.State.DataEntries is null or { Length: < 1 })
            throw new InvalidOperationException("Cannot calculate min when no data entries exist.");

        return Task.FromResult(_persistedState.State.DataEntries
            .Select(e => e.Value)
            .Min());
    }

    public Task<double> GetMax()
    {
        if (_persistedState.State.DataEntries is null or { Length: < 1 })
            throw new InvalidOperationException("Cannot calculate max when no data entries exist.");

        return Task.FromResult(_persistedState.State.DataEntries
            .Select(e => e.Value)
            .Max());
    }
}
