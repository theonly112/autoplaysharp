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
            _repo = repo;
            _game = game;
            _settings = settings;
        }

        private async void RunCommand()
        {
            try
            {
                var type = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.IsDynamic)
                    .SelectMany(a => a.GetTypes())
                    .FirstOrDefault(t => t.FullName.Equals(TaskName));
                var task = (IGameTask)Activator.CreateInstance(type, _game, _repo, _settings);
                await task.Run(CancellationToken.None);
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
    }
}
