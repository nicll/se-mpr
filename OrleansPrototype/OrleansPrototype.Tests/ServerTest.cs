using Microsoft.Extensions.Logging;
using Moq;
using OrleansPrototype.GrainImplementations;
using SharedLibraries.Plots;

namespace OrleansPrototype.Tests;

[TestClass]
public sealed class ServerTest
{
    [TestMethod]
    public async Task SampleSiloTest()
    {
        var persistentState = Mock.Of<IPersistentState<StatePersistence.PersistentSensorState>>();
        var plotGenerator = Mock.Of<IPlotGenerator>();
        var logger = Mock.Of<ILogger<SensorGrain>>();
        var grain = new SensorGrain(persistentState, plotGenerator, logger);

        await grain.DeleteData();
    }
}
