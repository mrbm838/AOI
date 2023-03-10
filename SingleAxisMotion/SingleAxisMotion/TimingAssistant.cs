using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SingleAxisMotion
{
    public class TimingAssistant
    {
        private TimingAssistant() { Cycle(); }

        private static volatile TimingAssistant _instance = new TimingAssistant();

        private static readonly object _locker = new object();

        public static TimingAssistant Instance => _instance;

        private readonly Stopwatch _timer = new Stopwatch();

        public bool Enable { get; set; } = false;

        public int Interval { get; set; } = 0;

        public void Start()
        {
            Enable = true;
            _timer.Start();
        }

        private void Cycle()
        {
            Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    Thread.Sleep(1);
                    if (Enable)
                    {
                        if (_timer.ElapsedMilliseconds > Interval)
                        {
                            _timer.Reset();
                            Enable = false;
                        }
                    }
                    else
                    {
                        if (_timer.IsRunning)
                        {
                            _timer.Reset();
                        }
                    }
                }
            });
        }

    }
}
