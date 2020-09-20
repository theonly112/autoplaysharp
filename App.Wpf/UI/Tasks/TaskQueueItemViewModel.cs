using autoplaysharp.Contracts;
using Prism.Commands;
using System.Windows.Input;

namespace autoplaysharp.App.UI.Tasks
{

    public class TaskQueueItemViewModel
    {
        private readonly IGameTask _task;
        private readonly ITaskExecutioner _executioner;

        public TaskQueueItemViewModel(IGameTask task, ITaskExecutioner executioner)
        {
            _task = task;
            Cancel = new DelegateCommand(CancelTask);
            _executioner = executioner;
            Name = task.GetType().Name;
        }

        private void CancelTask()
        {
            _executioner.Cancel(_task);
        }

        public string Name { get; set; }
        public ICommand Cancel { get; set; }
    }
}
