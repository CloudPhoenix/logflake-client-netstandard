using System.Diagnostics;

namespace NLogFlake
{
    public class PerformanceCounter
    {
        private readonly LogFlake _instance;
        private readonly string _label;
        private readonly Stopwatch _internalSw;

        private bool AlreadySent { get; set; }

        public PerformanceCounter(LogFlake instance, string label)
        {
            _instance = instance;
            _label = label;
            _internalSw = Stopwatch.StartNew();
        }

        ~PerformanceCounter()
        {
            if (!AlreadySent) Stop();
        }

        public void Start() => _internalSw.Start();
        public void Restart() => _internalSw.Restart();
        public long Stop() => Stop(true);
        public long Pause() => Stop(false);

        private long Stop(bool shouldSend)
        {
            _internalSw.Stop();
            if (!shouldSend) return _internalSw.ElapsedMilliseconds;
            AlreadySent = true;
            _instance.SendPerformance(_label, _internalSw.ElapsedMilliseconds);
            return _internalSw.ElapsedMilliseconds;
        }
    }
}
