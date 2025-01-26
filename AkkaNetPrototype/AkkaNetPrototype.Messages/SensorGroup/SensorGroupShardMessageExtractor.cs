using Akka.Cluster.Sharding;

namespace AkkaNetPrototype.Messages.SensorGroup;

public class SensorGroupShardMessageExtractor : HashCodeMessageExtractor
{
    public SensorGroupShardMessageExtractor(int maxNumberOfShards) : base(maxNumberOfShards)
    {
    }

    public override string? EntityId(object message)
    {
        if (message is ISensorGroupMessage sensorGroupMessage)
            return sensorGroupMessage.EntityId;

        return null;
    }
}
