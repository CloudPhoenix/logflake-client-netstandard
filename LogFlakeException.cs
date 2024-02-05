using System.Runtime.Serialization;

namespace LogFlake
{
    [Serializable]
    public class LogFlakeException : ApplicationException
    {
        public LogFlakeException() { }
        public LogFlakeException(string message) : base(message) { }
        public LogFlakeException(string message, Exception innerException) : base(message, innerException) { }
        protected LogFlakeException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}
