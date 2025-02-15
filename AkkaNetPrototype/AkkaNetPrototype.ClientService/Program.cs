using Akka.Cluster.Hosting;
using Akka.Discovery.Azure;
using Akka.Hosting;
using Akka.Management;
using Akka.Management.Cluster.Bootstrap;
using Akka.Remote.Hosting;
using AkkaNetPrototype.Messages.Sensor;
using AkkaNetPrototype.Messages.SensorGroup;
using SharedLibraries.SensorDataParser;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAkka("mpr-akkanet-a", builder =>
{
    builder
        .ConfigureLoggers(logConfig => logConfig.AddLoggerFactory())
        .WithRemoting(hostname: "localhost", port: 2551) // uses default 0.0.0.0:2552
        .WithClustering(new()
        {
            Roles = ["client"],
            SeedNodes = ["akka.tcp://mpr-akkanet-a@localhost:2552", "akka.tcp://mpr-akkanet-a@localhost:2551"]
        })
        //.WithAkkaManagement(port: 8557)
        //.WithClusterBootstrap(serviceName: "mpr-akkanet-a", requiredContactPoints: 1, newClusterEnabled: true)
        //.WithAzureDiscovery("DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://127.0.0.1:10000/devstoreaccount1;QueueEndpoint=http://127.0.0.1:10001/devstoreaccount1;TableEndpoint=http://127.0.0.1:10002/devstoreaccount1;", serviceName: "mpr-akkanet-a", publicPort: 8557)
        .WithShardRegionProxy<ISensorMessage>("sensors", "server", new SensorShardMessageExtractor(20)) // only needed on client, doesn't require access to actor type!
        .WithShardRegionProxy<ISensorGroupMessage>("sensorgroups", "server", new SensorGroupShardMessageExtractor(20));
});

builder.Services.AddSingleton<IDataParser, DataParser>();

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
