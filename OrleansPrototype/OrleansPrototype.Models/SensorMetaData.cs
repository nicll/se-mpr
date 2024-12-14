namespace OrleansPrototype.Models;

[GenerateSerializer]
[Alias("OrleansPrototype.Models.SensorMetaData")]
public class SensorMetaData
{
    [Id(0)]
    public required string Unit { get; init; }

    [Id(1)]
    public required string StationName { get; init; }

    [Id(2)]
    public required string ParameterName { get; init; }

    [Id(3)]
    public required double Wgs84Longitude { get; init; }

    [Id(4)]
    public required float Wgs84Latitude { get; init; }
}
