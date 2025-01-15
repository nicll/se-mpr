using Akka.Actor;
using AkkaNetPrototype.Messages.Sensor;
using SharedLibraries.Plots;

namespace AkkaNetPrototype.ServerService.Actors;

public class SensorActor : ReceiveActor
{
    private readonly ILogger<SensorActor> _logger;
    private readonly IPlotGenerator _plotGenerator;
    private readonly PersistentSensorState _persistedState;

    public SensorActor(ILogger<SensorActor> logger, IPlotGenerator plotGenerator)
    {
        _logger = logger;
        _plotGenerator = plotGenerator;
        ReceiveAsync<SetSensorConfiguration>(SetSensorConfiguration);
        ReceiveAsync<SetSensorMetadata>(SetSensorMetadata);
        ReceiveAsync<AppendSensorDataEntry>(AppendDataEntry);
        Receive<GetStatisticRequest>(GetStatistic);
        Receive<GetHistoryImageRequest>(GetHistoryImage);
        ReceiveAsync<DeleteData>(DeleteData);
    }

    private async Task SetSensorConfiguration(SetSensorConfiguration configuration)
    {
        _persistedState.MaxNumberOfRetainedDataEntries = configuration.Configuration.MaxNumberOfRetainedDataEntries;
        _persistedState.HistoryImageWidth = configuration.Configuration.HistoryImageWidth;
        _persistedState.HistoryImageHeight = configuration.Configuration.HistoryImageHeight;
        // ToDo: persist
    }

    private async Task SetSensorMetadata(SetSensorMetadata metadata)
    {
        _persistedState.Unit = metadata.Metadata.Unit;
        _persistedState.StationName = metadata.Metadata.StationName;
        _persistedState.ParameterName = metadata.Metadata.ParameterName;
        _persistedState.Wgs84Longitude = metadata.Metadata.Wgs84Longitude;
        _persistedState.Wgs84Latitude = metadata.Metadata.Wgs84Latitude;
        // ToDo: persist
    }

    private async Task AppendDataEntry(AppendSensorDataEntry dataEntry)
    {
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
        // ToDo: persist
    }

    // requires callback to sender
    private void GetStatistic(GetStatisticRequest request)
    {
        try
        {
            if (_persistedState.DataEntries is null or { Length: < 1 })
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
        if (_persistedState.DataEntries is null or { Length: < 1 })
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

    private async Task DeleteData(DeleteData _)
    {
        // ToDo: unpersist
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

    public required string Unit { get; set; }

    public required string StationName { get; set; }

    public required string ParameterName { get; set; }

    public required double Wgs84Longitude { get; set; }

    public required float Wgs84Latitude { get; set; }

    public required PersistentSensorDataEntryState[] DataEntries { get; set; }

    public required int MaxNumberOfRetainedDataEntries { get; set; } = DefaultMaxNumberOfRetainedEntries;

    public required int HistoryImageWidth { get; set; } = DefaultHistoryImageWidth;

    public required int HistoryImageHeight { get; set; } = DefaultHistoryImageHeight;
}

internal class PersistentSensorDataEntryState
{
    public required double Value { get; init; }

    public required DateTimeOffset MeasuredAt { get; init; }

    public required double Quality { get; init; }
}
