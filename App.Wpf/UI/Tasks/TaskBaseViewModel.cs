using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;
using Prism.Commands;
using System;
using System.Windows.Input;

namespace autoplaysharp.App.UI.Tasks
{
    internal abstract class TaskBaseViewModel
    {
        protected abstract IGameTask CreateTask();
        public abstract string Name { get; }
        public abstract ICommand AddToQueue { get; }
    }

    internal class TaskBaseViewModel<Task> : TaskBaseViewModel where Task : IGameTask
    {
        private readonly IGame _game;
        private readonly IUiRepository _repo;
        private readonly ITaskExecutioner _taskExecutioner;
        private readonly ISettings _settings;

        public TaskBaseViewModel(IGame game, IUiRepository repo, ITaskExecutioner taskExecutioner, ISettings settings)
        {
            _game = game;
            _repo = repo;
            _taskExecutioner = taskExecutioner;
            _settings = settings;
            AddToQueue = new DelegateCommand(Add);
        }

        private void Add()
        {
            _taskExecutioner.QueueTask(CreateTask());
        }

        protected override IGameTask CreateTask()
        {
            return (Task)Activator.CreateInstance(typeof(Task), _game, _repo, _settings);
        }

        public override string Name => typeof(Task).Name;

        public override ICommand AddToQueue { get; }
    }
}
