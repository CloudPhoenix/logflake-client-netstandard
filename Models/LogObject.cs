using System.Text.Json.Serialization;

namespace LogFlake.Models
{
	internal class LogObject
    {
        [JsonPropertyName("datetime")]
        public DateTime Date = DateTime.UtcNow;

        [JsonPropertyName("hostname")]
        public string? Hostname { get; set; }

        [JsonPropertyName("level")]
        public string InternalLevel { get => Level.ToString(); }

        [JsonIgnore]
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
    }
}
