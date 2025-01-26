using Akka.Cluster.Sharding;

namespace AkkaNetPrototype.Messages.Sensor;

// this determines how the shard region knows to which shard and entity to route the message
// each entity (=actor) resides in exactly one shard inside the shard region
public class SensorShardMessageExtractor : HashCodeMessageExtractor
{
    public SensorShardMessageExtractor(int maxNumberOfShards) : base(maxNumberOfShards)
    {
    }

    public override string? EntityId(object message)
    {
        if (message is ISensorMessage sensorMessage)
            return sensorMessage.EntityId;

        return null;
    }
}
