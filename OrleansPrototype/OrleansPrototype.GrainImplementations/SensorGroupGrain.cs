using Microsoft.Extensions.Logging;
using OrleansPrototype.GrainInterfaces;
using OrleansPrototype.Models;
using OrleansPrototype.StatePersistence;

namespace OrleansPrototype.GrainImplementations;

public class SensorGroupGrain : Grain, ISensorGroupGrain
{
    private readonly IPersistentState<PersistentSensorGroupState> _persistedState;
    private readonly IGrainFactory _grainFactory;
    private readonly ILogger<SensorGroupGrain> _logger;

    public SensorGroupGrain(
        [PersistentState(nameof(PersistentSensorGroupState))] IPersistentState<PersistentSensorGroupState> persistedState,
        IGrainFactory grainFactory,
        ILogger<SensorGroupGrain> logger)
    {
        _persistedState = persistedState;
        _grainFactory = grainFactory;
        _logger = logger;
    }

    public Task SetMetaData(SensorGroupMetaData sensorGroupMetaData)
    {
        throw new NotImplementedException();
    }

    public async Task LinkSensors(LinkSensorsData linkSensorsData)
    {
        if (_persistedState.RecordExists)
            throw new InvalidOperationException("Cannot link existing group to sensors.");

        // initialize all grains
        foreach (var sensor in linkSensorsData.Sensors)
        {
            var grain = _grainFactory.GetGrain<ISensorGrain>(sensor.SensorId.NumericIdentifier, sensor.SensorId.TypeIdentifier);
            await grain.SetMetaData(sensor.MetaData);
            await grain.ConfigureSensor(sensor.Configuration);
        }

        // remember that these sensors/grains belong to this group
        _persistedState.State.LinkedSensors = linkSensorsData.Sensors
            .Select(e => (e.SensorId.NumericIdentifier, e.SensorId.TypeIdentifier))
            .ToArray();

        await _persistedState.WriteStateAsync();
    }

    public async Task UnlinkSensors()
    {
        if (!_persistedState.RecordExists || _persistedState.State.LinkedSensors is null or { Length: < 1 })
            throw new InvalidOperationException("Cannot unlink non-existing group.");

        foreach (var sensor in _persistedState.State.LinkedSensors)
        {
            var grain = _grainFactory.GetGrain<ISensorGrain>(sensor.numericIdentifier, sensor.typeIdentifier);
            await grain.DeleteData();
        }

        await _persistedState.ClearStateAsync();
    }
}
