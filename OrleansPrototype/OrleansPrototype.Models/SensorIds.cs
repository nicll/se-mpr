﻿namespace OrleansPrototype.Models;

[GenerateSerializer]
[Alias("OrleansPrototype.Models.SensorIds")]
public class SensorIds
{
    public required ICollection<SensorId> Ids { get; init; }
}

[GenerateSerializer]
[Alias("OrleansPrototype.Models.SensorId")]
public record class SensorId(long NumericIdentifier, string TypeIdentifier);