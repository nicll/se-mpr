using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Microsoft.Extensions.Logging;
using Proto;
using Proto.Cluster;
using Proto.Persistence;
using ProtoActorPrototype.Grains;
using ProtoActorPrototype.Persistence;
using SharedLibraries.Plots;

namespace ProtoActorPrototype.GrainImplementations;

public class SensorGrainImplementation : SensorGrainBase
{
    private readonly ILogger<SensorGrainImplementation> _logger;
    private readonly IPlotGenerator _plotGenerator;
    private readonly Proto.Persistence.Persistence _persistence;
    private PersistentSensorState _persistedState;

    public SensorGrainImplementation(IContext context, ISnapshotStore snapshotStore, ClusterIdentity clusterIdentity, IPlotGenerator plotGenerator, ILogger<SensorGrainImplementation> logger) : base(context)
    {
        _persistence = Proto.Persistence.Persistence.WithSnapshotting(snapshotStore, clusterIdentity.Identity, ApplySnapshot);
        _plotGenerator = plotGenerator;
        _persistedState = null!; // initialized using the below method before anything else gets executed
        _logger = logger;
    }

    private void ApplySnapshot(Snapshot snapshot)
    {
        if (snapshot.State is PersistentSensorState state)
        {
            _persistedState = state;
        }
        else
        {
            _logger.LogError("Failed to recover grain state.");
        }
    }

    public override async Task OnStarted()
    {
        await _persistence.RecoverStateAsync();

        // first time use of this grain
        _persistedState ??= new()
        {
            DataEntries = [],
            MaxNumberOfRetainedDataEntries = PersistentSensorState.DefaultMaxNumberOfRetainedEntries,
            HistoryImageHeight = PersistentSensorState.DefaultHistoryImageHeight,
            HistoryImageWidth = PersistentSensorState.DefaultHistoryImageWidth,
            ParameterName = "unconfigured",
            StationName = "unconfigured",
            Unit = "unconfigured",
            Wgs84Latitude = default,
            Wgs84Longitude = default
        };
    }

    public override async Task AppendDataEntry(SensorDataEntry request)
    {
        // prepare the data we're handling
        var maxNumberOfRetainedEntries = _persistedState.MaxNumberOfRetainedDataEntries;
        var persistentDataEntry = new PersistentSensorDataEntryState
        {
            Value = request.Value,
            MeasuredAt = request.MeasuredAt.ToDateTimeOffset(),
            Quality = request.Quality
        };

        var newEntries = (_persistedState.DataEntries ?? []).Append(persistentDataEntry);

        // remove the first few entries if we have too many
        if (newEntries.Count() > maxNumberOfRetainedEntries)
            newEntries = newEntries.Skip(newEntries.Count() - maxNumberOfRetainedEntries);

        // swap out data entries and save
        _persistedState.DataEntries = newEntries.ToArray();
        await _persistence.PersistSnapshotAsync(_persistedState);
    }

    public override async Task SetMetaData(SensorMetaData request)
    {
        _persistedState.Unit = request.Unit;
        _persistedState.StationName = request.StationName;
        _persistedState.ParameterName = request.ParameterName;
        _persistedState.Wgs84Longitude = request.Wgs84Longitude;
        _persistedState.Wgs84Latitude = request.Wgs84Latitude;
        await _persistence.PersistSnapshotAsync(_persistedState);
    }

    public override async Task ConfigureSensor(SensorConfiguration request)
    {
        _persistedState.MaxNumberOfRetainedDataEntries = request.MaxNumberOfRetainedDataEntries;
        _persistedState.HistoryImageWidth = request.HistoryImageWidth;
        _persistedState.HistoryImageHeight = request.HistoryImageHeight;
        await _persistence.PersistSnapshotAsync(_persistedState);
    }

    public override Task<DoubleValue> GetAverage()
    {
        if (_persistedState.DataEntries is null or { Length: < 1 })
            throw new InvalidOperationException("Cannot calculate average when no data entries exist.");

        return Task.FromResult(new DoubleValue
        {
            Value = _persistedState.DataEntries
                .Select(e => e.Value)
                .Average()
        });
    }

    public override Task<SensorHistoryImage> GetHistoryImage()
    {
        if (_persistedState.DataEntries is null or { Length: < 1 })
            throw new InvalidOperationException("Cannot generate history image when no data entries exist.");

        var pngImage = _plotGenerator.GeneratePngPlot(
            _persistedState.StationName + " - " + _persistedState.ParameterName,
            _persistedState.Unit,
            _persistedState.DataEntries.Select(s => (s.MeasuredAt, s.Value)).ToArray(),
            _persistedState.HistoryImageWidth,
            _persistedState.HistoryImageHeight);

        return Task.FromResult(new SensorHistoryImage
        {
            PngImage = ByteString.CopyFrom(pngImage)
        });
    }

    public override Task<DoubleValue> GetMaximum()
    {
        if (_persistedState.DataEntries is null or { Length: < 1 })
            throw new InvalidOperationException("Cannot calculate max when no data entries exist.");

        return Task.FromResult(new DoubleValue
        {
            Value = _persistedState.DataEntries
                .Select(e => e.Value)
                .Max()
        });
    }

    public override Task<DoubleValue> GetMinimum()
    {
        if (_persistedState.DataEntries is null or { Length: < 1 })
            throw new InvalidOperationException("Cannot calculate min when no data entries exist.");

        return Task.FromResult(new DoubleValue
        {
            Value = _persistedState.DataEntries
                .Select(e => e.Value)
                .Min()
        });
    }

    public override async Task DeleteData()
    {
        await _persistence.DeleteSnapshotsAsync(_persistence.Index);
    }
}
