using autoplaysharp.Contracts;
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

        public DebugViewModel(IGame game, IUiRepository repo)
        {
            Run = new DelegateCommand(RunCommand);
            _repo = repo;
            _game = game;
        }

        private void RunCommand()
        {
            try
            {
                var type = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.IsDynamic)
                    .SelectMany(a => a.GetTypes())
                    .FirstOrDefault(t => t.FullName.Equals(TaskName));
                var task = (IGameTask)Activator.CreateInstance(type, _game, _repo);
                task.Run(CancellationToken.None);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
            }
        }

        public string TaskName { get; set; }
        public ICommand Run { get; }
    }
}
