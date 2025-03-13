using Microsoft.Extensions.Logging;
using Moq;
using Proto;
using Proto.Cluster;
using Proto.Persistence;
using ProtoActorPrototype.GrainImplementations;
using SharedLibraries.Plots;

namespace ProtoActorPrototype.Tests;

[TestClass]
public class ServerTest
{
    [TestMethod]
    public async Task SampleSiloTest()
    {
        var context = Mock.Of<IContext>();
        var snapshotStore = Mock.Of<ISnapshotStore>();
        var clusterIdentity = ClusterIdentity.Create("a", "b");
        var plotGenerator = Mock.Of<IPlotGenerator>();
        var logger = Mock.Of<ILogger<SensorGrainImplementation>>();
        var grain = new SensorGrainImplementation(context, snapshotStore, clusterIdentity, plotGenerator, logger);

        await grain.DeleteData();
    }
}
