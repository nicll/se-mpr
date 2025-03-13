namespace ProtoActorPrototype.Tests;

[TestClass]
public sealed class ClientTest
{

    [TestMethod]
    public void SampleClientTest()
    {
        // there is no interface for the SensorGrainClient
        // also, the "Cluster" class itself does not have a corresponding interface that can be implemented with a mock object
        // and since the GetSensorGrain method is a static extension method, the linking can also not be redirected to a different implementation

        // thus, it is not possible to create a mock of the client for a unit test
    }
}
