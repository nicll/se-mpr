using Akka.Actor;
using AkkaNetPrototype.Messages.Sensor;
using AkkaNetPrototype.Messages.SensorGroup;

namespace AkkaNetPrototype.ServerService.Actors;

public class SensorGroupActor : ReceiveActor
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<SensorGroupActor> _logger;
    private readonly PersistentSensorGroupState _persistedState;

    public SensorGroupActor(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _logger = serviceProvider.GetRequiredService<ILogger<SensorGroupActor>>();
        Receive<SetSensorGroupMetadata>(SetSensorGroupMetadata);
        Receive<GetSensorIdsRequest>(ListSensors);
        ReceiveAsync<LinkSensors>(LinkSensors);
        ReceiveAsync<UnlinkSensors>(UnlinkSensors);
    }

    private void SetSensorGroupMetadata(SetSensorGroupMetadata _)
        => throw new NotImplementedException();

    private void ListSensors(GetSensorIdsRequest _)
    {
        if (_persistedState.LinkedSensors is null or { Length: < 1 })
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
        if (_persistedState.LinkedSensors is { Length: > 0 })
            throw new InvalidOperationException("Cannot link existing group to sensors.");

        // initialize all child actors
        foreach (var sensor in linkSensors.Sensors)
        {
            var actor = Context.ActorOf(SensorActor.GetProps(_serviceProvider), sensor.SensorId.NumericIdentifier + '#' + sensor.SensorId.TypeIdentifier);
            actor.Tell(new SetSensorMetadata { Metadata = sensor.Metadata }, Self);
            actor.Tell(new SetSensorConfiguration { Configuration = sensor.Configuration }, Self);
        }

        // remember that these sensors/grains belong to this group
        _persistedState.LinkedSensors = linkSensors.Sensors
            .Select(e => (e.SensorId.NumericIdentifier, e.SensorId.TypeIdentifier))
            .ToArray();

        // ToDo: persist
    }

    private async Task UnlinkSensors(UnlinkSensors _)
    {
        if (_persistedState.LinkedSensors is null or { Length: < 1 })
        {
            Sender.Tell(new Status.Failure(new InvalidOperationException("Cannot unlink non-existing group.")), Self);
            return;
        }

        foreach (var sensor in _persistedState.LinkedSensors)
        {
            var actor = Context.Child(sensor.numericIdentifier + '#' + sensor.typeIdentifier);
            actor.Tell(new DeleteData(), Self);
            actor.Tell(PoisonPill.Instance, Self);
        }

        _persistedState.LinkedSensors = [];
        // ToDo: unpersist child actors
    }
}

internal class PersistentSensorGroupState
{
    public required (long numericIdentifier, string typeIdentifier)[] LinkedSensors { get; set; }
}
