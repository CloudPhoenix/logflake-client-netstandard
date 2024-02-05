using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace LogFlake.Models
{
	internal class LogObject
    {
        [JsonProperty("datetime")]
        public DateTime Date = DateTime.UtcNow;

        [JsonProperty("hostname")]
        public string Hostname { get; set; }

        [JsonProperty("level")]
        public string InternalLevel { get => Level.ToString(); }

        [JsonIgnore]
        public LogLevels Level { get; set; }

        [JsonProperty("content")]
        public string Content { get; set; }

        [JsonProperty("correlation")]
        public string Correlation { get; set; }

        [JsonProperty("params")]
        public Dictionary<string, object> Parameters { get; set; }

        [JsonProperty("label")]
        public string Label { get; set; }

        [JsonProperty("duration")]
        public long Duration { get; set; }
        
        public override string ToString()
        {
            return JsonConvert.SerializeObject(this, Formatting.None);
        }
    }
}
