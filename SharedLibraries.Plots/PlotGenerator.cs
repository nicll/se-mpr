using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.SkiaSharp;

namespace SharedLibraries.Plots;

public class PlotGenerator : IPlotGenerator
{
    public byte[] GeneratePngPlot(string sensorName, string unit, ICollection<(DateTimeOffset pointInTime, double yValue)> dataSet, int width, int height)
    {
        var plotModel = new PlotModel
        {
            Title = "Sensor History",
            Subtitle = sensorName
        };

        plotModel.Axes.Add(new DateTimeAxis
        {
            Position = AxisPosition.Bottom,
            StringFormat = "yy-MM-dd hh:mm",
            Title = "Measurement time",
            MajorGridlineStyle = LineStyle.Solid,
            MinorGridlineStyle = LineStyle.Dot
        });

        plotModel.Axes.Add(new LinearAxis
        {
            Position = AxisPosition.Left,
            Title = "Measured value in " + unit,
            MajorGridlineStyle = LineStyle.Solid,
            MinorGridlineStyle = LineStyle.Dot
        });

        var scatterSeries = new ScatterSeries
        {
            MarkerType = MarkerType.Circle,
            MarkerSize = 4,
            MarkerStroke = OxyColors.Green
        };

        foreach (var (pointInTime, yValue) in dataSet)
            scatterSeries.Points.Add(new ScatterPoint(DateTimeAxis.ToDouble(pointInTime.LocalDateTime), yValue));

        plotModel.Series.Add(scatterSeries);

        using var ms = new MemoryStream();
        PngExporter.Export(plotModel, ms, width, height);

        return ms.ToArray();
    }
}
