using Google.Protobuf.WellKnownTypes;
using Proto.Cluster.Partition;
using Proto.Cluster;
using Proto.Remote.GrpcNet;
using Proto.Remote.HealthChecks;
using Proto;
using ProtoActorPrototype.Grains;
using ProtoActorPrototype.ClientService;
using Proto.Remote;
using Proto.Cluster.Testing;
using Proto.DependencyInjection;
using Proto.OpenTelemetry;
using Proto.Cluster.Cache;
using Proto.Cluster.Consul;
using SharedLibraries.SensorDataParser;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
                .WithClusterKinds(Array.Empty<ClusterKind>())
                .WithHeartbeatExpirationDisabled() // needed when using breakpoints
            )
            .Cluster()
            .WithPidCacheInvalidation();

        return system;
    });

    services.AddSingleton(sp => sp.GetRequiredService<ActorSystem>().Cluster());

    services.AddHostedService<ActorSystemHostedService>();

    services.AddHealthChecks().AddCheck<ActorSystemHealthCheck>("as-hc");

    // no persistence config needed on client
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
