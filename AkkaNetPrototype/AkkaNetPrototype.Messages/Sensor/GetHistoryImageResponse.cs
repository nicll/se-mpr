namespace AkkaNetPrototype.Messages.Sensor;

public class GetHistoryImageResponse
{
    public required byte[] PngImage { get; init; }
}
