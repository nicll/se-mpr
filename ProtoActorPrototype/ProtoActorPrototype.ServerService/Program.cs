using Google.Protobuf.WellKnownTypes;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Proto;
using Proto.Cluster;
using Proto.Cluster.Cache;
using Proto.Cluster.Consul;
using Proto.Cluster.Partition;
using Proto.Cluster.Testing;
using Proto.DependencyInjection;
using Proto.OpenTelemetry;
using Proto.Persistence;
using Proto.Persistence.MongoDB;
using Proto.Persistence.SqlServer;
using Proto.Remote;
using Proto.Remote.GrpcNet;
using Proto.Remote.HealthChecks;
using ProtoActorPrototype.GrainImplementations;
using ProtoActorPrototype.Grains;
using ProtoActorPrototype.Persistence;
using ProtoActorPrototype.ServerService;
using SharedLibraries.Plots;
using SharedLibraries.SensorDataParser;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<IPlotGenerator, PlotGenerator>();
builder.Services.AddSingleton<IDataParser, DataParser>();

AddProtoActorSetup(builder.Services);

builder.WebHost.UseKestrel(opts => opts.AllowSynchronousIO = true);

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

static void AddProtoActorSetup(IServiceCollection services)
{
    services.AddSingleton(provider =>
    {
        const string clusterName = "mpr_protoactor_a";
        var clusterKinds = new[]
        {
            SensorGrainActor.GetClusterKind((c, ci) => ActivatorUtilities.CreateInstance<SensorGrainImplementation>(provider, c, ci)),
            SensorGroupGrainActor.GetClusterKind((c, ci) => ActivatorUtilities.CreateInstance<SensorGroupGrainImplementation>(provider, c, ci))
        };
        var config = provider.GetRequiredService<IConfiguration>();

        Log.SetLoggerFactory(provider.GetRequiredService<ILoggerFactory>());

        var actorSystemConfig = ActorSystemConfig
            .Setup()
            .WithMetrics()
            .WithConfigureRootContext(context => context.WithTracing());

        actorSystemConfig
            .WithDeveloperSupervisionLogging(true)
            .WithDeadLetterRequestLogging(true)
            .WithDeadLetterResponseLogging(true)
            .WithDeveloperThreadPoolStatsLogging(true);

        var system = new ActorSystem(actorSystemConfig);

        var (remoteConfig, clusterProvider) = ConfigureClustering(config);

        system
            .WithServiceProvider(provider)
            .WithRemote(remoteConfig)
            .WithCluster(ClusterConfig
                .Setup(clusterName, clusterProvider, new PartitionIdentityLookup())
                .WithClusterKinds(clusterKinds)
                .WithHeartbeatExpirationDisabled() // needed when using breakpoints
            )
            .Cluster()
            .WithPidCacheInvalidation();

        return system;
    });

    services.AddSingleton(sp => sp.GetRequiredService<ActorSystem>().Cluster());

    services.AddHostedService<ActorSystemHostedService>();

    services.AddHealthChecks().AddCheck<ActorSystemHealthCheck>("as-hc");

    // add provider for persistence
    //services.AddSingleton<ISnapshotStore, MongoDBProvider>(sp =>
    //{
    //    var mongoClient = new MongoClient("mongodb://localhost:27017/mpr_protoactor_a");
    //    var mongoDatabase = mongoClient.GetDatabase("mpr_protoactor_a");
    //    return new MongoDBProvider(mongoDatabase);
    //});
    //// required starting with MongoDB client v2.19
    //var objectSerializer = new ObjectSerializer(type => ObjectSerializer.DefaultAllowedTypes(type) || type.FullName!.StartsWith(nameof(ProtoActorPrototype)));
    //BsonSerializer.RegisterSerializer(objectSerializer);
    //BsonClassMap.RegisterClassMap<PersistentSensorGroupState>();
    //BsonClassMap.RegisterClassMap<PersistentSensorState>();
    //BsonClassMap.RegisterClassMap<PersistentSensorDataEntryState>();

    services.AddSingleton<ISnapshotStore, SqlServerProvider>(sp =>
    {
        return new SqlServerProvider(@"Data Source=(localdb)\MSSQLLocalDB;" +
            "Initial Catalog=mpr_protoactor_a;Integrated Security=True;" +
            "Pooling=False;Max Pool Size=200;MultipleActiveResultSets=True;Encrypt=False;TrustServerCertificate=True",
            autoCreateTables: true);
    });
}

static (GrpcNetRemoteConfig, IClusterProvider) ConfigureClustering(IConfiguration config)
{
    return (GrpcNetRemoteConfig
        .BindToLocalhost()
        .WithProtoMessages([EmptyReflection.Descriptor, WrappersReflection.Descriptor, MessagesReflection.Descriptor])
        .WithRemoteDiagnostics(true),
        //new TestProvider(new TestProviderOptions(), new InMemAgent()));
        new ConsulProvider(new ConsulProviderConfig().WithDeregisterCritical(TimeSpan.FromMinutes(5)).WithServiceTtl(TimeSpan.FromMinutes(1)), client => client.Address = new("http://localhost:8500")));
}
