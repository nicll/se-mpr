using Akka.Actor;
using Akka.Persistence;
using AkkaNetPrototype.Messages.Sensor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharedLibraries.Plots;

namespace AkkaNetPrototype.Actors;

public class SensorActor : ReceivePersistentActor
{
    private readonly ILogger<SensorActor> _logger;
    private readonly IPlotGenerator _plotGenerator;
    private PersistentSensorState? _persistedState;
    private long _persistenceSequenceNumber;

    public override string PersistenceId => nameof(SensorActor) + '#' + Context.Self.Path.Name; // unique ID

    public SensorActor(IServiceProvider serviceProvider)
    {
        _logger = serviceProvider.GetRequiredService<ILogger<SensorActor>>();
        _plotGenerator = serviceProvider.GetRequiredService<IPlotGenerator>();
        Recover<SnapshotOffer>(RecoverState);
        Command<SaveSnapshotSuccess>(DeleteOldStates);
        Command<SetSensorConfiguration>(SetSensorConfiguration);
        Command<SetSensorMetadata>(SetSensorMetadata);
        Command<AppendSensorDataEntry>(AppendDataEntry);
        Command<GetStatisticRequest>(GetStatistic);
        Command<GetHistoryImageRequest>(GetHistoryImage);
        Command<DeleteData>(DeleteData);
    }

    private void RecoverState(SnapshotOffer snapshotOffer)
    {
        if (snapshotOffer.Snapshot is PersistentSensorState state)
        {
            _persistedState = state;
            _persistenceSequenceNumber = snapshotOffer.Metadata.SequenceNr;
        }
    }

    private void DeleteOldStates(SaveSnapshotSuccess success)
    {
        _persistenceSequenceNumber = success.Metadata.SequenceNr;
        DeleteSnapshots(new(_persistenceSequenceNumber - 1));
    }

    private void SetSensorConfiguration(SetSensorConfiguration configuration)
    {
        _persistedState ??= new();
        _persistedState.MaxNumberOfRetainedDataEntries = configuration.Configuration.MaxNumberOfRetainedDataEntries;
        _persistedState.HistoryImageWidth = configuration.Configuration.HistoryImageWidth;
        _persistedState.HistoryImageHeight = configuration.Configuration.HistoryImageHeight;
        SaveSnapshot(_persistedState);
    }

    private void SetSensorMetadata(SetSensorMetadata metadata)
    {
        _persistedState ??= new();
        _persistedState.Unit = metadata.Metadata.Unit;
        _persistedState.StationName = metadata.Metadata.StationName;
        _persistedState.ParameterName = metadata.Metadata.ParameterName;
        _persistedState.Wgs84Longitude = metadata.Metadata.Wgs84Longitude;
        _persistedState.Wgs84Latitude = metadata.Metadata.Wgs84Latitude;
        SaveSnapshot(_persistedState);
    }

    private void AppendDataEntry(AppendSensorDataEntry dataEntry)
    {
        _persistedState ??= new();
        // prepare the data we're handling
        var maxNumberOfRetainedEntries = _persistedState.MaxNumberOfRetainedDataEntries;
        var persistentDataEntry = new PersistentSensorDataEntryState
        {
            Value = dataEntry.Value,
            MeasuredAt = dataEntry.MeasuredAt,
            Quality = dataEntry.Quality
        };

        var newEntries = (_persistedState.DataEntries ?? []).Append(persistentDataEntry);

        // remove the first few entries if we have too many
        if (newEntries.Count() > maxNumberOfRetainedEntries)
            newEntries = newEntries.Skip(newEntries.Count() - maxNumberOfRetainedEntries);

        // swap out data entries and save
        _persistedState.DataEntries = newEntries.ToArray();
        SaveSnapshot(_persistedState);
    }

    // requires callback to sender
    private void GetStatistic(GetStatisticRequest request)
    {
        try
        {
            if (_persistedState?.DataEntries is null or { Length: < 1 })
                throw new InvalidOperationException("Cannot calculate statistic when no data entries exist.");

            var values = _persistedState.DataEntries
                .Select(e => e.Value);

            var value = request.Type switch
            {
                StatisticType.Average => values.Average(),
                StatisticType.Min => values.Min(),
                StatisticType.Max => values.Max(),
                _ => throw new InvalidOperationException("Unknown statistic type.")
            };

            Sender.Tell(new GetStatisticResponse
            {
                StatisticType = request.Type,
                Value = value
            }, Self);
        }
        catch (Exception e)
        {
            Sender.Tell(new Status.Failure(e), Self);
        }
    }

    private void GetHistoryImage(GetHistoryImageRequest _)
    {
        if (_persistedState?.DataEntries is null or { Length: < 1 })
        {
            Sender.Tell(new Status.Failure(new InvalidOperationException("Cannot generate history image when no data entries exist.")), Self);
            return;
        }

        var pngImage = _plotGenerator.GeneratePngPlot(
            _persistedState.StationName + " - " + _persistedState.ParameterName,
            _persistedState.Unit,
            _persistedState.DataEntries.Select(s => (s.MeasuredAt, s.Value)).ToArray(),
            _persistedState.HistoryImageWidth,
            _persistedState.HistoryImageHeight);

        Sender.Tell(new GetHistoryImageResponse
        {
            PngImage = pngImage
        }, Self);
    }

    private void DeleteData(DeleteData _)
    {
        _persistedState = new();
        DeleteSnapshots(new(_persistenceSequenceNumber));
        _persistenceSequenceNumber = 0;
    }

    public static Props GetProps(IServiceProvider serviceProvider)
        => Props.Create<SensorActor>(serviceProvider);
}

internal class PersistentSensorState
{
    public const int
        DefaultMaxNumberOfRetainedEntries = 40,
        DefaultHistoryImageWidth = 800,
        DefaultHistoryImageHeight = 600;

    public string Unit { get; set; } = "";

    public string StationName { get; set; } = "unnamed";

    public string ParameterName { get; set; } = "unnamed";

    public double Wgs84Longitude { get; set; }

    public double Wgs84Latitude { get; set; }

    public PersistentSensorDataEntryState[]? DataEntries { get; set; }

    public int MaxNumberOfRetainedDataEntries { get; set; } = DefaultMaxNumberOfRetainedEntries;

    public int HistoryImageWidth { get; set; } = DefaultHistoryImageWidth;

    public int HistoryImageHeight { get; set; } = DefaultHistoryImageHeight;
}

internal class PersistentSensorDataEntryState
{
    public double Value { get; init; }

    public DateTimeOffset MeasuredAt { get; init; }

    public double Quality { get; init; }
}
