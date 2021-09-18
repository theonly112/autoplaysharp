using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;
using Prism.Commands;
using System;
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

        public DebugViewModel(IGame game, IUiRepository repo, ISettings settings)
        {
            Run = new DelegateCommand(RunCommand);
            Drag = new DelegateCommand(ExecuteDrag);
            _repo = repo;
            _game = game;
            _settings = settings;
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

        public ICommand Drag
        {
            get;
        }
    }
}
