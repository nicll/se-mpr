using Microsoft.Extensions.Logging;
using Proto;
using Proto.Cluster;
using Proto.Persistence;
using ProtoActorPrototype.Grains;
using ProtoActorPrototype.Persistence;

namespace ProtoActorPrototype.GrainImplementations;

public class SensorGroupGrainImplementation : SensorGroupGrainBase
{
    private readonly ILogger<SensorGrainImplementation> _logger;
    private readonly Proto.Persistence.Persistence _persistence;
    private PersistentSensorGroupState _persistedState;

    public SensorGroupGrainImplementation(IContext context, ISnapshotStore snapshotStore, ClusterIdentity clusterIdentity, ILogger<SensorGrainImplementation> logger) : base(context)
    {
        _persistence = Proto.Persistence.Persistence.WithSnapshotting(snapshotStore, clusterIdentity.Identity, ApplySnapshot);
        _persistedState = null!; // initialized using the below method before anything else gets executed
        _logger = logger;
    }

    private void ApplySnapshot(Snapshot snapshot)
    {
        if (snapshot.State is PersistentSensorGroupState state)
        {
            _persistedState = state;
        }
        else
        {
            _logger.LogError("Failed to recover grain state.");
        }
    }

    public override async Task OnStarted()
    {
        await _persistence.RecoverStateAsync();

        _persistedState ??= new()
        {
            LinkedSensors = []
        };
    }

    public override Task SetMetaData(SensorGroupMetaData request)
    {
        throw new NotImplementedException();
    }

    public override Task<SensorIds> ListSensors()
    {
        if (_persistedState.LinkedSensors is null or [])
            return Task.FromResult(new SensorIds());

        return Task.FromResult(new SensorIds
        {
            Ids =
            {
                _persistedState.LinkedSensors
                    .Select(t => new SensorId { NumericIdentifier = t.numericIdentifier, TypeIdentifier = t.typeIdentifier })
            }
        });
    }

    public override async Task LinkSensors(LinkSensorsData request)
    {
        if (_persistedState.LinkedSensors is { Length: > 0 })
            throw new InvalidOperationException("Cannot link existing group to sensors.");

        foreach (var sensor in request.Sensors)
        {
            var grain = Context
                .GetSensorGrain(sensor.SensorId.NumericIdentifier + '/' + sensor.SensorId.TypeIdentifier);
            await grain.SetMetaData(sensor.MetaData, CancellationToken.None);
            await grain.ConfigureSensor(sensor.Configuration, CancellationToken.None);
        }

        _persistedState.LinkedSensors = request.Sensors
            .Select(e => (e.SensorId.NumericIdentifier, e.SensorId.TypeIdentifier))
            .ToArray();

        await _persistence.PersistSnapshotAsync(_persistedState);
    }

    public override async Task UnlinkSensors()
    {
        if (_persistedState.LinkedSensors is null or { Length: < 1 })
            throw new InvalidOperationException("Cannot unlink non-existing group.");

        foreach (var sensor in _persistedState.LinkedSensors)
        {
            var grain = Context.GetSensorGrain(sensor.numericIdentifier + '/' +  sensor.typeIdentifier);
            await grain.DeleteData(CancellationToken.None);
        }

        _persistedState.LinkedSensors = [];
        await _persistence.DeleteSnapshotsAsync(_persistence.Index);
    }
}
