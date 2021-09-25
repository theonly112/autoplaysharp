using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;
using Prism.Commands;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace autoplaysharp.App.UI.DebugView
{
    internal class DebugViewModel
    {
        private readonly IUiRepository _repo;
        private readonly IGame _game;
        private readonly ISettings _settings;
        private readonly IVideoCapture _capture;

        public DebugViewModel(IGame game,
            IUiRepository repo,
            ISettings settings,
            IVideoCapture capture)
        {
            Run = new DelegateCommand(RunCommand);
            Drag = new DelegateCommand(ExecuteDrag);
            StartRecording = new DelegateCommand(ExecuteStartRecording);
            EndRecording = new DelegateCommand(ExecuteEndRecording);
            _repo = repo;
            _game = game;
            _settings = settings;
            _capture = capture;
        }

        private void ExecuteEndRecording()
        {
            _capture.End();
        }

        private void ExecuteStartRecording()
        {
            _capture.Start("Debug Recording");
        }

        private void ExecuteDrag()
        {
            _game.Drag(UIds.MAIN_MENU_SELECT_MISSION_DRAG_RIGHT,
                UIds.MAIN_MENU_SELECT_MISSION_DRAG_LEFT);
        }

        private async void RunCommand()
        {
            try
            {
                var type = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.IsDynamic)
                    .SelectMany(a => a.GetTypes())
                    .FirstOrDefault(t => t.FullName != null && t.FullName.Equals(TaskName));
                var task = (IGameTask)Activator.CreateInstance(type ?? throw new InvalidOperationException(), _game, _repo, _settings);
                if (task != null) await task.Run(CancellationToken.None);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        public string TaskName { get; set; }
        public ICommand Run { get; }

        public bool EnableOverlay
        {
            get => _settings.EnableOverlay;
            set => _settings.EnableOverlay = value;
        }

        public bool RestartOnError
        {
            get => _settings.RestartOnError;
            set => _settings.RestartOnError = value;
        }

        public bool EnableRecording
        {
            get => _settings.VideoCapture.Enabled;
            set => _settings.VideoCapture.Enabled = value;
        }

        public string FrameRate
        {
            get => _settings.VideoCapture.FrameRate.ToString(CultureInfo.InvariantCulture);
            set
            {
                if (double.TryParse(value, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var newValue))
                {
                    _settings.VideoCapture.FrameRate = newValue;
                }
            }
        }

        public ICommand Drag
        {
            get;
        }

        public ICommand StartRecording
        {
            get;
        }

        public ICommand EndRecording
        {
            get;
        }

        public string RecordingDirectory
        {
            get => _settings.VideoCapture.RecordingDir;
            set
            {
                if (!Directory.Exists(value))
                {
                    return;
                }
                _settings.VideoCapture.RecordingDir = value;
            }
        }
    }
}
