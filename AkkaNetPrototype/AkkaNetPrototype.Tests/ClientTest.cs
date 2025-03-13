using Akka.Actor;
using Akka.Hosting;
using AkkaNetPrototype.ClientService.Controllers;
using AkkaNetPrototype.Messages.Sensor;
using Microsoft.Extensions.Logging;
using Moq;
using SharedLibraries.SensorDataParser;

namespace AkkaNetPrototype.Tests;

[TestClass]
public class ClientTest
{
    [TestMethod]
    public async Task SampleClientTest()
    {
        var logger = Mock.Of<ILogger<SensorController>>();
        var actorRegistryMock = new Mock<IActorRegistry>();
        actorRegistryMock.Setup(m => m.GetAsync<ISensorMessage>(default))
            .ReturnsAsync(Mock.Of<IActorRef>());
        var dataParser = Mock.Of<IDataParser>();
        var controller = new SensorController(logger, actorRegistryMock.Object, dataParser);

        await controller.AddData(0, "a", 1, default, 0);
    }
}
