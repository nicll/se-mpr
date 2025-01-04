using Proto;
using Proto.Cluster;

namespace ProtoActorPrototype.ClientService;

public class ActorSystemHostedService : IHostedService
{
    private readonly ActorSystem _actorSystem;
    private readonly ILogger<ActorSystemHostedService> _logger;

    public ActorSystemHostedService(ActorSystem actorSystem, ILogger<ActorSystemHostedService> logger)
    {
        _actorSystem = actorSystem;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting actor system.");

        await _actorSystem
            .Cluster()
            .StartClientAsync();
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping actor system.");

        await _actorSystem
            .Cluster()
            .ShutdownAsync();
    }
}
