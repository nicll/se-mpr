namespace SharedLibraries.SensorDataParser;

public interface IDataParser
{
    Task<(long numericIdentifier, string typeIdentifier, DateTimeOffset measurementTime, double value)[]> LoadValues(Stream dataStream);
}
