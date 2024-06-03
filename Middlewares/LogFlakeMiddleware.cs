using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using NLogFlake.Constants;
using NLogFlake.Helpers;
using NLogFlake.Services;

namespace NLogFlake.Middlewares;

public class LogFlakeMiddleware
{
    private readonly RequestDelegate _next;

    public LogFlakeMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext, ILogFlakeService logFlakeService, ICorrelationService correlationService)
    {
        LogFlakeMiddlewareHelper.ValidateArguments(httpContext, logFlakeService, correlationService);

        httpContext.Request.EnableBuffering();

        string fullPath = httpContext.Request.Path;

        Uri uri = new(fullPath);
        bool ignoreLogProcessing = logFlakeService.Settings.ExcludedPaths.Contains(GetInitialPath(uri));

        string correlation = correlationService.Correlation;
        string parentCorrelation = httpContext.Request.Headers[HttpContextConstants.ParentCorrelationHeader].ToString();
        if (string.IsNullOrWhiteSpace(parentCorrelation))
        {
            httpContext.Request.Headers[HttpContextConstants.ParentCorrelationHeader] = correlation;
        }

        IPerformanceCounter? performance = logFlakeService.Settings.PerformanceMonitor && !ignoreLogProcessing ? logFlakeService.MeasurePerformance(fullPath) : null;

        await _next(httpContext);

        string? client = HttpContextHelper.GetClientId(httpContext);

        if (httpContext.Response.StatusCode >= StatusCodes.Status400BadRequest)
        {
            if (!ignoreLogProcessing)
            {
                string logMessage = LogFlakeMiddlewareHelper.GetLogErrorMessage(fullPath, client, httpContext.Response);

                logFlakeService.WriteLog(LogLevels.ERROR, logMessage.ToString(), correlation);
            }

            if (httpContext.Response.ContentLength is null && httpContext.Items[HttpContextConstants.HasCatchedError] is null)
            {
                await httpContext.Response.WriteAsync(JsonSerializer.Serialize(new
                {
                    Error = new
                    {
                        ErrorCode = httpContext.Response.StatusCode,
                        ErrorMessage = ((HttpStatusCode)httpContext.Response.StatusCode).ToString()
                    },
                    RequestStatus = "KO",
                }), CancellationToken.None);
            }
        }

        if (logFlakeService.Settings.AutoLogRequest && !ignoreLogProcessing)
        {
            string logMessage = LogFlakeMiddlewareHelper.GetLogMessage(fullPath, client, httpContext.Response, performance, parentCorrelation);

            Dictionary<string, object> content = await HttpContextHelper.GetLogParametersAsync(httpContext, logFlakeService.Settings.AutoLogResponse);

            logFlakeService.WriteLog(LogLevels.INFO, logMessage.ToString(), correlation, content);
        }
    }

    private static string GetInitialPath(Uri uri)
    {
        string initialPath = string.Join(string.Empty, uri.Segments.Take(3));

        return uri.Segments.Length > 3
            ? initialPath.Substring(0, initialPath.Length - 1)
            : initialPath;
    }
}
