namespace SharedLibraries.Plots;

public interface IPlotGenerator
{
    byte[] GeneratePngPlot(string sensorName, string unit, ICollection<(DateTimeOffset pointInTime, double yValue)> dataSet, int width, int height);
}
