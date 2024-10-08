using System;
using System.Diagnostics;

namespace WebsocketServer
{
    public class TimeHelper
    {
        private Stopwatch _timer;

        public TimeHelper() 
        { 
            _timer = new Stopwatch();
        }

        public void Start() => _timer.Restart();
        public double Stop()
        {
            _timer.Stop();
            TimeSpan elapsed = _timer.Elapsed;
            return elapsed.TotalMilliseconds;
        }
    }
}
