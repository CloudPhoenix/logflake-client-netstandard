using System.Text.Json;
using System.Text.Json.Serialization;

namespace NLogFlake.Models;

internal class LogObject
{
    [JsonPropertyName("datetime")]
    public DateTime Date = DateTime.UtcNow;

    [JsonPropertyName("hostname")]
    public string? Hostname { get; set; }

    [JsonPropertyName("level")]
    public LogLevels Level { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("correlation")]
    public string? Correlation { get; set; }

    [JsonPropertyName("params")]
    public Dictionary<string, object>? Parameters { get; set; }

    [JsonPropertyName("label")]
    public string? Label { get; set; }

    [JsonPropertyName("duration")]
    public long Duration { get; set; }

    public override string ToString() => JsonSerializer.Serialize(this);
}
