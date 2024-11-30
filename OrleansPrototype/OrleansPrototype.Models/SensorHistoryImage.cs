namespace OrleansPrototype.Models;

[GenerateSerializer]
[Alias("OrleansPrototype.Models.SensorHistoryImage")]
public class SensorHistoryImage
{
    [Id(0)]
    public required byte[] PngImage { get; init; }
}
