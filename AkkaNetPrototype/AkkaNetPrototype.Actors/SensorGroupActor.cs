using Akka.Actor;
using Akka.Hosting;
using Akka.Persistence;
using AkkaNetPrototype.Messages.Sensor;
using AkkaNetPrototype.Messages.SensorGroup;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace AkkaNetPrototype.Actors;

public class SensorGroupActor : ReceivePersistentActor
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ActorRegistry _actorRegistry;
    private readonly ILogger<SensorGroupActor> _logger;
    private PersistentSensorGroupState? _persistedState;
    private long _persistenceSequenceNumber;

    public override string PersistenceId => nameof(SensorGroupActor) + '#' + Context.Self.Path.Name; // unique ID

    public SensorGroupActor(IServiceProvider serviceProvider, ActorRegistry actorRegistry)
    {
        _serviceProvider = serviceProvider;
        _actorRegistry = actorRegistry;
        _logger = serviceProvider.GetRequiredService<ILogger<SensorGroupActor>>();
        Recover<SnapshotOffer>(RecoverState);
        Command<SaveSnapshotSuccess>(DeleteOldStates);
        Command<SetSensorGroupMetadata>(SetSensorGroupMetadata);
        Command<GetSensorIdsRequest>(ListSensors);
        CommandAsync<LinkSensors>(LinkSensors);
        CommandAsync<UnlinkSensors>(UnlinkSensors);
    }

    private void RecoverState(SnapshotOffer snapshotOffer)
    {
        if (snapshotOffer.Snapshot is PersistentSensorGroupState state)
        {
            _persistedState = state;
            _persistenceSequenceNumber = snapshotOffer.Metadata.SequenceNr;
        }
    }

    private void DeleteOldStates(SaveSnapshotSuccess success)
    {
        _persistenceSequenceNumber = success.Metadata.SequenceNr;
        DeleteSnapshots(new(_persistenceSequenceNumber - 1));
    }

    private void SetSensorGroupMetadata(SetSensorGroupMetadata _)
        => throw new NotImplementedException();

    private void ListSensors(GetSensorIdsRequest _)
    {
        if (_persistedState?.LinkedSensors is null or { Length: < 1 })
        {
            Sender.Tell(new Status.Failure(new InvalidOperationException("No linked sensors.")), Self);
            return;
        }

        Sender.Tell(new GetSensorIdsResponse
        {
            Ids = _persistedState.LinkedSensors
                .Select(t => new SensorId(t.numericIdentifier, t.typeIdentifier))
                .ToArray()
        }, Self);
    }

    private async Task LinkSensors(LinkSensors linkSensors)
    {
        if (_persistedState?.LinkedSensors is null or { Length: > 0 })
            throw new InvalidOperationException("Cannot link existing group to sensors.");

        // initialize all child actors
        foreach (var sensor in linkSensors.Sensors)
        {
            var sensorActorShard = await _actorRegistry.GetAsync<ISensorMessage>();
            sensorActorShard.Tell(new SetSensorMetadata { EntityId = sensor.SensorId.NumericIdentifier + '#' + sensor.SensorId.TypeIdentifier, Metadata = sensor.Metadata }, Self);
            sensorActorShard.Tell(new SetSensorConfiguration { EntityId = sensor.SensorId.NumericIdentifier + '#' + sensor.SensorId.TypeIdentifier, Configuration = sensor.Configuration }, Self);
        }

        // remember that these sensors/grains belong to this group
        _persistedState.LinkedSensors = linkSensors.Sensors
            .Select(e => (e.SensorId.NumericIdentifier, e.SensorId.TypeIdentifier))
            .ToArray();

        SaveSnapshot(_persistedState);
    }

    private async Task UnlinkSensors(UnlinkSensors _)
    {
        if (_persistedState?.LinkedSensors is null or { Length: < 1 })
        {
            Sender.Tell(new Status.Failure(new InvalidOperationException("Cannot unlink non-existing group.")), Self);
            return;
        }

        foreach (var sensor in _persistedState.LinkedSensors)
        {
            var sensorActorShard = await _actorRegistry.GetAsync<ISensorMessage>();
            sensorActorShard.Tell(new DeleteData { EntityId = sensor.numericIdentifier + '#' + sensor.typeIdentifier }, Self);
            sensorActorShard.Tell(PoisonPill.Instance, Self);
        }

        _persistedState.LinkedSensors = [];
        DeleteSnapshots(new(_persistenceSequenceNumber));
        _persistenceSequenceNumber = 0;
    }
}

internal class PersistentSensorGroupState
{
    public required (long numericIdentifier, string typeIdentifier)[] LinkedSensors { get; set; }
}
