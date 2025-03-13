using Moq;
using OrleansPrototype.GrainInterfaces;

namespace OrleansPrototype.Tests;

[TestClass]
public class ClientTest
{

    [TestMethod]
    public void SampleClientTest()
    {
        var clusterClientMock = new Mock<IClusterClient>();
        var grainMock = new Mock<ISensorGrain>();

        clusterClientMock
            .Setup(m => m.GetGrain<ISensorGrain>(It.IsAny<long>(), It.IsAny<string>(), null))
            .Returns(grainMock.Object);

        // this clusterClient can be injected into a controller using the usual DI pattern
        var clusterClient = clusterClientMock.Object;
        var grain = clusterClient.GetGrain<ISensorGrain>(123, "456");

        Assert.IsNotNull(grain);
    }
}
