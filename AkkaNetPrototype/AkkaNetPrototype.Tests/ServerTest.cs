using Akka.Actor;
using Akka.TestKit;
using Akka.TestKit.MsTest;
using AkkaNetPrototype.Actors;
using AkkaNetPrototype.Messages.Sensor;
using Microsoft.Extensions.Logging;
using Moq;
using SharedLibraries.Plots;

namespace AkkaNetPrototype.Tests;

[TestClass]
public sealed class ServerTest : TestKit
{
    [TestMethod]
    public void TestMethod1()
    {
        var spMock = new Mock<IServiceProvider>();
        spMock.Setup(m => m.GetService(typeof(ILogger<SensorActor>)))
            .Returns(Mock.Of<ILogger<SensorActor>>());
        spMock.Setup(m => m.GetService(typeof(IPlotGenerator)))
            .Returns(Mock.Of<IPlotGenerator>());
        var actor = ActorOfAsTestActorRef<SensorActor>(Props.Create(() => new SensorActor(spMock.Object)));

        Within(TimeSpan.FromSeconds(10), () =>
        {
            // currently broken due to https://github.com/akkadotnet/akka.net/issues/7254
            // increase expected exception count to 1 when fixed
            EventFilter.Exception<InvalidOperationException>().Expect(0, () =>
            {
                var resp = actor.Ask<GetStatisticResponse>(new GetStatisticRequest { EntityId = "x", Type = StatisticType.Average });
            });
        });
    }
}
