using Akka.Cluster.Hosting;
using Akka.Cluster.Sharding;
using Akka.Discovery.Azure;
using Akka.Hosting;
using Akka.Management;
using Akka.Management.Cluster.Bootstrap;
using Akka.Persistence.MongoDb.Hosting;
using Akka.Persistence.SqlServer.Hosting;
using Akka.Remote.Hosting;
using AkkaNetPrototype.Actors;
using AkkaNetPrototype.Messages.Sensor;
using AkkaNetPrototype.Messages.SensorGroup;
using SharedLibraries.Plots;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAkka("mpr-akkanet-a", builder =>
{
    builder
        .ConfigureLoggers(logConfig => logConfig.AddLoggerFactory())
        .WithRemoting(hostname: "localhost", port: 2552) // uses default 0.0.0.0:2552
        //.WithInMemorySnapshotStore() // for db-less testing
        .WithSqlServerPersistence(@"Data Source=(localdb)\MSSQLLocalDB;" +
            "Initial Catalog=mpr_akkanet_a;Integrated Security=True;" +
            "Pooling=False;Max Pool Size=200;MultipleActiveResultSets=True;Encrypt=False;TrustServerCertificate=True")
        //.WithMongoDbPersistence("mongodb://localhost:27017/mpr_akkanet_a")
        .WithClustering(new()
        {
            Roles = ["server"],
            SeedNodes = ["akka.tcp://mpr-akkanet-a@localhost:2552", "akka.tcp://mpr-akkanet-a@localhost:2551"]
        })
        //.WithAkkaManagement(port: 8558)
        //.WithClusterBootstrap(serviceName: "mpr-akkanet-a", requiredContactPoints: 1, newClusterEnabled: true)
        //.WithAzureDiscovery("DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;", serviceName: "mpr-akkanet-a", publicPort: 8558)
        // messages get routed to the respective actor based to their entity id (custom id field that is part of message)
        .WithShardRegion<ISensorMessage>("sensors", (sys, reg, res) => s => res.Props<SensorActor>(),
            new SensorShardMessageExtractor(20), // 10 * nodes_count
            new ShardOptions
            {
                Role = "server",
                RememberEntities = false, // automatically start moved entities upon startup of new node / rebalance
                StateStoreMode = StateStoreMode.Persistence, // or DData
                PassivateIdleEntityAfter = TimeSpan.FromMinutes(5)
            })
        .WithShardRegion<ISensorGroupMessage>("sensorgroups", (sys, reg, res) => s => res.Props<SensorGroupActor>(),
            new SensorGroupShardMessageExtractor(20), // 10 * nodes_count
            new ShardOptions
            {
                Role = "server",
                RememberEntities = false, // automatically start moved entities upon startup of new node / rebalance
                StateStoreMode = StateStoreMode.Persistence, // or DData
                PassivateIdleEntityAfter = TimeSpan.FromMinutes(5)
            });
});

builder.Services.AddSingleton<IPlotGenerator, PlotGenerator>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
