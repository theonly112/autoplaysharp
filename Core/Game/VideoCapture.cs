using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using Size = OpenCvSharp.Size;

namespace autoplaysharp.Core.Game
{
    public class VideoCapture : IVideoCapture, IDisposable
    {
        private readonly IEmulatorWindow _window;
        private readonly IVideoProvider _provider;
        private readonly ITaskQueue _taskQueue;
        private readonly ISettings _settings;
        private VideoWriter _writer;
        private readonly object _lock = new();

        public VideoCapture(IEmulatorWindow window,
            IVideoProvider provider,
            ITaskQueue taskQueue,
            ISettings settings)
        {
            _window = window;
            _provider = provider;
            _taskQueue = taskQueue;
            _settings = settings;
            _taskQueue.PropertyChanged += TaskQueueOnPropertyChanged;
            _provider.OnNewFrame += ProviderOnOnNewFrame;
        }

        private void TaskQueueOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(ITaskQueue.ActiveItem)) return;
            
            var activeItem = _taskQueue.ActiveItem;
            if (activeItem != null)
            {
                Start(activeItem.GetType().Name);
            }
            else
            {
                End();
            }
        }

        private void ProviderOnOnNewFrame(Bitmap obj)
        {
            lock (_lock)
            {
                if (!_settings.VideoCapture.Enabled)
                {
                    return;
                }

                if (_writer == null) return;
                using var mat = obj.ToMat();
                _writer.Write(mat);
            }
        }

        public void Start(string sessionName)
        {
            lock (_lock)
            {
                if (!_settings.VideoCapture.Enabled)
                {
                    return;
                }

                _writer = CreateWriter(sessionName);
            }
        }
        private VideoWriter CreateWriter(string sessionName)
        {
            var size = new Size(_window.Width, _window.Height);
            if (!Directory.Exists(_settings.VideoCapture.RecordingDir))
            {
                Directory.CreateDirectory(_settings.VideoCapture.RecordingDir);
            }
            var fileName = Path.Combine(_settings.VideoCapture.RecordingDir,
                $"{DateTime.UtcNow:yyyy-dd-M--HH-mm-ss} - {sessionName}.mp4");
            var writer = new VideoWriter(fileName, 
                FourCC.MP4V, 
                _settings.VideoCapture.FrameRate, 
                size);

            return writer;
        }

        public void End()
        {
            lock (_lock)
            {
                _writer?.Release();
                _writer?.Dispose();
                _writer = null;
            }
        }

        public void Dispose()
        {
            _provider.OnNewFrame -= ProviderOnOnNewFrame;
            _writer?.Dispose();
        }
    }
}
