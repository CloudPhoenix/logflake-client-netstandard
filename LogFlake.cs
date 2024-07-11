using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using NLogFlake.Constants;
using NLogFlake.Models;
using NLogFlake.Models.Options;
using Snappier;

namespace NLogFlake;

public class LogFlake : ILogFlake
{
    private Uri Server { get; set; }
    private string? _hostname = Environment.MachineName;
    private string AppId { get; set; }

    private readonly ConcurrentQueue<PendingLog> _logsQueue = new();
    private readonly ManualResetEvent _processLogs = new(false);
    private readonly IHttpClientFactory _httpClientFactory;

    private Thread LogsProcessorThread { get; set; }
    private bool IsShuttingDown { get; set; }

    internal int FailedPostRetries { get; set; } = 3;
    internal bool EnableCompression { get; set; } = true;

    public void SetHostname() => SetHostname(null);

    public string? GetHostname() => _hostname;

    public void SetHostname(string? hostname) => _hostname = string.IsNullOrWhiteSpace(hostname) ? null : hostname;

    public LogFlake(IOptions<LogFlakeOptions> logFlakeOptions, IHttpClientFactory httpClientFactory)
    {
        AppId = logFlakeOptions.Value.AppId!;

        Server = logFlakeOptions.Value.Endpoint ?? new Uri(ServersConstants.PRODUCTION);

        LogsProcessorThread = new Thread(LogsProcessor);
        LogsProcessorThread.Start();

        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    [Obsolete("Do not use this constructor, if you cannot instantiate with Dependency Injection, use the .NET Framework package.", error: true)]
    public LogFlake(LogFlakeOptions logFlakeOptions, IHttpClientFactory httpClientFactory)
    {
        AppId = logFlakeOptions.AppId!;

        Server = logFlakeOptions.Endpoint ?? new Uri(ServersConstants.PRODUCTION);

        LogsProcessorThread = new Thread(LogsProcessor);
        LogsProcessorThread.Start();

        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    ~LogFlake() => Shutdown();

    public void Shutdown()
    {
        IsShuttingDown = true;
        LogsProcessorThread.Join();
    }

    private void LogsProcessor()
    {
        SendLog(LogLevels.DEBUG, $"LogFlake started on {_hostname}");

        _processLogs.WaitOne();

        while (!_logsQueue.IsEmpty)
        {
            _ = _logsQueue.TryDequeue(out PendingLog? log);
            log.Retries++;
            bool success = Post(log.QueueName!, log.JsonString!).GetAwaiter().GetResult();
            if (!success && log.Retries < FailedPostRetries)
            {
                _logsQueue.Enqueue(log);
            }

            _processLogs.Reset();

            if (_logsQueue.IsEmpty && !IsShuttingDown)
            {
                _processLogs.WaitOne();
            }
        }
    }

    private async Task<bool> Post(string queueName, string jsonString)
    {
        if (queueName != QueuesConstants.LOGS && queueName != QueuesConstants.PERFORMANCES)
        {
            return false;
        }

        try
        {
            string requestUri = $"/api/ingestion/{AppId}/{queueName}";
            HttpResponseMessage result = new(System.Net.HttpStatusCode.InternalServerError);
            using HttpClient httpClient = _httpClientFactory.CreateClient(HttpClientConstants.ClientName);
            httpClient.BaseAddress = Server;
            if (EnableCompression)
            {
                byte[] jsonStringBytes = Encoding.UTF8.GetBytes(jsonString);
                string base64String = Convert.ToBase64String(jsonStringBytes);
                byte[] compressed = Snappy.CompressToArray(Encoding.UTF8.GetBytes(base64String));
                ByteArrayContent content = new(compressed);
                content.Headers.Remove("Content-Type");
                content.Headers.Add("Content-Type", "application/octet-stream");
                result = await httpClient.PostAsync(requestUri, content);
            }
            else
            {
                StringContent content = new(jsonString, Encoding.UTF8, "application/json");
                result = await httpClient.PostAsync(requestUri, content);
            }

            return result.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public void SendLog(string content, Dictionary<string, object>? parameters = null) => SendLog(LogLevels.DEBUG, content, parameters);

    public void SendLog(LogLevels level, string content, Dictionary<string, object>? parameters = null) => SendLog(level, null, content, parameters);

    public void SendLog(LogLevels level, string? correlation, string? content, Dictionary<string, object>? parameters = null)
    {
        _logsQueue.Enqueue(new PendingLog
        {
            QueueName = QueuesConstants.LOGS,
            JsonString = new LogObject
            {
                Level = level,
                Hostname = GetHostname(),
                Content = content!,
                Correlation = correlation,
                Parameters = parameters,
            }.ToString()
        });

        _processLogs.Set();
    }

    public void SendException(Exception e) => SendException(e, null);

    public void SendException(Exception e, string? correlation)
    {
        StringBuilder additionalTrace = new();
        if (e.Data.Count > 0)
        {
            additionalTrace.Append($"{Environment.NewLine}Data:");
            additionalTrace.Append($"{Environment.NewLine}{JsonSerializer.Serialize(e.Data, new JsonSerializerOptions { WriteIndented = true })}");
        }

        _logsQueue.Enqueue(new PendingLog
        {
            QueueName = QueuesConstants.LOGS,
            JsonString = new LogObject
            {
                Level = LogLevels.EXCEPTION,
                Hostname = GetHostname(),
                Content = $"{e.Demystify()}{additionalTrace}",
                Correlation = correlation,
            }.ToString()
        });

        _processLogs.Set();
    }

    public void SendPerformance(string label, long duration)
    {
        _logsQueue.Enqueue(new PendingLog
        {
            QueueName = QueuesConstants.PERFORMANCES,
            JsonString = new LogObject
            {
                Label = label,
                Duration = duration,
            }.ToString()
        });

        _processLogs.Set();
    }

    public IPerformanceCounter MeasurePerformance(string label) => new PerformanceCounter(this, label);
}
