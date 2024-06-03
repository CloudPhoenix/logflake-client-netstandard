namespace NLogFlake.Models;

public sealed class LogFlakeSettings
{
    public bool AutoLogRequest { get; set; }

    public bool AutoLogResponse { get; set; }

    public bool AutoLogUnauthorized { get; set; }

    public bool AutoLogGlobalExceptions { get; set; }

    public bool PerformanceMonitor { get; set; }

    public IEnumerable<string>? ExcludedPaths { get; set; }
}
