namespace NLogFlake.Services;

public class LogFlakeService : ILogFlakeService
{
    private readonly ILogFlake _logFlake;

    private readonly string _version;

    public LogFlakeService(ILogFlake logFlake, IVersionService versionService)
    {
        _logFlake = logFlake;

        _version = versionService.Version;
    }

    public void WriteLog(LogLevels logLevel, string? message, string? correlation, Dictionary<string, object>? parameters = null)
    {
        parameters?.Add("Assembly version", _version);

        _logFlake.SendLog(logLevel, correlation, message, parameters);
    }

    public void WriteException(Exception ex, string? correlation, string? message = null, Dictionary<string, object>? parameters = null)
    {
        _logFlake.SendException(ex, correlation);

        WriteLog(LogLevels.FATAL, string.IsNullOrWhiteSpace(message) ? (ex?.Message ?? string.Empty) : $"{message}\n{ex?.Message ?? string.Empty}", correlation, parameters);
    }

    public IPerformanceCounter MeasurePerformance(string label) => _logFlake.MeasurePerformance(label);

    public bool SendPerformance(string label, long duration)
    {
        try
        {
            _logFlake.SendPerformance(label, duration);
        }
        catch (ObjectDisposedException)
        {
            return false;
        }

        return true;
    }

}
