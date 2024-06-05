namespace NLogFlake.Services;

public interface ILogFlakeService
{
    void WriteLog(LogLevels logLevel, string? message, string? correlation, Dictionary<string, object>? parameters = null);

    void WriteException(Exception ex, string? correlation, string? message = null, Dictionary<string, object>? parameters = null);

    IPerformanceCounter MeasurePerformance(string label);

    bool SendPerformance(string label, long duration);
}
