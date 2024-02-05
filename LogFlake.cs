using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LogFlake.Models;
using Newtonsoft.Json;

namespace LogFlake
{
    public class LogFlake
    {
        private string Server { get; set; } = Servers.PRODUCTION;
        private string _hostname = Environment.MachineName;
        private string AppId { get; set; }

        private readonly ConcurrentQueue<PendingLog> _logsQueue = new ConcurrentQueue<PendingLog>();
        private readonly ManualResetEvent _processLogs = new ManualResetEvent(false);
        private Thread LogsProcessorThread { get; set; }
        private bool IsShuttingDown { get; set; }

        public int FailedPostRetries { get; set; } = 3;
        public int PostTimeoutSeconds { get; set; } = 3;

        public void SetHostname() => SetHostname(null);

        public string GetHostname() => _hostname;

        public void SetHostname(string hostname) => _hostname = string.IsNullOrEmpty(hostname) ? null : hostname;

        public LogFlake(string appId, string logFlakeServer) : this(appId) => Server = logFlakeServer;

        public LogFlake(string appId)
        {
            if (appId.Length == 0) throw new LogFlakeException("appId missing");
            AppId = appId;
            LogsProcessorThread = new Thread(LogsProcessor);
            LogsProcessorThread.Start();
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
                // Process log
                _logsQueue.TryDequeue(out var log);
                log.Retries++;
                var success = Post(log.QueueName, log.JsonString).Result;
                if (!success && log.Retries < FailedPostRetries) _logsQueue.Enqueue(log);
                _processLogs.Reset();
                if (_logsQueue.IsEmpty && !IsShuttingDown) _processLogs.WaitOne();
            }
        }

        private async Task<bool> Post(string queueName, string jsonString)
        {
            if (queueName != Queues.LOGS && queueName != Queues.PERFORMANCES) return false;
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri($"{Server}");
                    client.Timeout = TimeSpan.FromSeconds(PostTimeoutSeconds);
                    var json = new StringContent(jsonString, Encoding.UTF8, "application/json");
                    var result = await client.PostAsync($"/api/ingestion/{AppId}/{queueName}", json);
                    return result.IsSuccessStatusCode;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void SendLog(string content, Dictionary<string, object> parameters = null) =>
            SendLog(LogLevels.DEBUG, content, parameters);

        public void SendLog(LogLevels level, string content, Dictionary<string, object> parameters = null) =>
            SendLog(level, null, content, parameters);

        public void SendLog(LogLevels level, string correlation, string content,
            Dictionary<string, object> parameters = null)
        {
            _logsQueue.Enqueue(new PendingLog
            {
                QueueName = Queues.LOGS,
                JsonString = new LogObject
                {
                    Level = level,
                    Hostname = GetHostname(),
                    Content = content,
                    Correlation = correlation,
                    Parameters = parameters
                }.ToString()
            });
            _processLogs.Set();
        }

        public void SendException(Exception e) =>
            SendException(e, null);

        public void SendException(Exception e, string correlation)
        {
            var additionalTrace = new StringBuilder();
            if (e.Data.Count > 0)
            {
                additionalTrace.Append($"{Environment.NewLine}Data:");
                additionalTrace.Append(
                    $"{Environment.NewLine}{JsonConvert.SerializeObject(e.Data, Formatting.Indented)}");
            }

            _logsQueue.Enqueue(new PendingLog
            {
                QueueName = Queues.LOGS,
                JsonString = new LogObject
                {
                    Level = LogLevels.EXCEPTION,
                    Hostname = GetHostname(),
                    Content = $"{e.Demystify()}{additionalTrace}",
                    Correlation = correlation
                }.ToString()
            });
            _processLogs.Set();
        }

        public void SendPerformance(string label, long duration)
        {
            _logsQueue.Enqueue(new PendingLog
            {
                QueueName = Queues.PERFORMANCES,
                JsonString = new LogObject
                {
                    Label = label,
                    Duration = duration
                }.ToString()
            });
            _processLogs.Set();
        }

        public PerformanceCounter MeasurePerformance(string label) => new PerformanceCounter(this, label);
    }
}