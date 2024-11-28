namespace OrleansPrototype.Models;

[GenerateSerializer]
public class SensorMetaData
{
    public required string Unit { get; init; }

    public required string StationName { get; init; }

    public required string ParameterName { get; init; }

    public required double Wgs84Longitude { get; init; }

    public required float Wgs84Latitude { get; init; }
}
