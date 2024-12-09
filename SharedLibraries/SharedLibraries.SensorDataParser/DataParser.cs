
using CsvHelper;

namespace SharedLibraries.SensorDataParser;

public class DataParser : IDataParser
{
    public async Task<(long numericIdentifier, string typeIdentifier, DateTimeOffset measurementTime, double value)[]> LoadValues(Stream dataStream)
    {
        using var streamReader = new StreamReader(dataStream);
        using var parser = new CsvReader(streamReader, System.Globalization.CultureInfo.InvariantCulture);
        var records = new List<(long numericIdentifier, string typeIdentifier, DateTimeOffset measurementTime, double value)>();

        await parser.ReadAsync();
        parser.ReadHeader();

        while (await parser.ReadAsync())
        {
            var measurementTime = parser.GetField<DateTimeOffset>(0);
            var stationId = parser.GetField<long>(1);

            for (int i = 2; i < parser.ColumnCount; ++i)
            {
                if (parser.TryGetField<double>(i, out var value))
                    records.Add((stationId, parser.HeaderRecord![i], measurementTime, value));
            }
        }

        return records.ToArray();
    }
}
