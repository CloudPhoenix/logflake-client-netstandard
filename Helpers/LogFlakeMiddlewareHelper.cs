using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using NLogFlake.Services;

namespace NLogFlake.Helpers;

internal static class LogFlakeMiddlewareHelper
{
    internal static string GetLogMessage(string fullPath, string? client, HttpResponse response, IPerformanceCounter? performance, string parentCorrelation)
    {
        StringBuilder logMessage = new($"Called {fullPath}");

        if (!string.IsNullOrWhiteSpace(client))
        {
            logMessage.Append($" by client {client}");
        }

        if (response is not null)
        {
            logMessage.Append($" with status code {response.StatusCode}");
        }

        if (performance is not null)
        {
            long time = performance.Stop();

            logMessage.Append($" and execution time {time:N0} ms");
        }

        if (!string.IsNullOrWhiteSpace(parentCorrelation))
        {
            logMessage.Append($" - parent-correlation: {parentCorrelation}");
        }

        return logMessage.ToString();
    }

    internal static string GetLogErrorMessage(string fullPath, string? client, HttpResponse response)
    {
        StringBuilder logMessage = new($"Error for method {fullPath}");

        if (!string.IsNullOrWhiteSpace(client))
        {
            logMessage.Append($" by client {client}");
        }

        logMessage.Append($" with status code {response.StatusCode} ({(HttpStatusCode)response.StatusCode})");

        return logMessage.ToString();
    }

    internal static string GetLogExceptionMessage(string? client, string? exceptionMessage)
    {
        StringBuilder logMessage = new($"Exception with error:\n{exceptionMessage ?? string.Empty}");

        if (!string.IsNullOrWhiteSpace(client))
        {
            logMessage.Insert(0, $"Client {client}. ");
        }

        return logMessage.ToString();
    }

    internal static void ValidateArguments(HttpContext httpContext, ILogFlakeService logFlakeService, ICorrelationService correlationService)
    {
        if (httpContext is null)
        {
            throw new ArgumentNullException(nameof(httpContext));
        }

        if (logFlakeService is null)
        {
            throw new ArgumentNullException(nameof(logFlakeService));
        }

        if (correlationService is null)
        {
            throw new ArgumentNullException(nameof(correlationService));
        }
    }
}
