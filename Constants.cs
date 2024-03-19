namespace NLogFlake
{
    public static class Servers
    {
        public const string PRODUCTION = "https://app.logflake.io";
        public const string TEST = "https://app-test.logflake.io";
    }

    public static class Queues
    {
        public const string LOGS = "logs";
        public const string PERFORMANCES = "perf";
    }

    public enum LogLevels
    {
        DEBUG     = 0,
        INFO      = 1,
        WARN      = 2,
        ERROR     = 3,
        FATAL     = 4,
        EXCEPTION = 5,
    }
}
