using System;
using System.Drawing;
using System.Timers;
using autoplaysharp.Contracts;

namespace autoplaysharp.Emulators
{
    public class VideoProvider : IVideoProvider, IDisposable
    {
        private readonly IEmulatorWindow _window;
        private readonly Timer _timer;
        private Bitmap _screen;
        private readonly object _lock = new();

        public VideoProvider(IEmulatorWindow window, double frameRate = 30)
        {
            _window = window;
            if (frameRate >= 1000)
            {
                throw new ArgumentException();
            }
            _timer = new Timer(1000 / frameRate);
            _timer.Elapsed += TimerOnElapsed;
            _timer.Enabled = true;
        }

        private void TimerOnElapsed(object sender, ElapsedEventArgs e)
        {
            lock (_lock)
            {
                _screen?.Dispose();
                _screen = _window.GrabScreen(0, 0, _window.Width, _window.Height);
                OnNewFrame?.Invoke(_screen);
            }
        }

        public Bitmap GetCurrentFrame()
        {
            lock (_lock)
            {
                return _screen;
            }
        }

        public event Action<Bitmap> OnNewFrame;

        public void Dispose()
        {
            _timer?.Dispose();
            _screen?.Dispose();
        }
    }
}
