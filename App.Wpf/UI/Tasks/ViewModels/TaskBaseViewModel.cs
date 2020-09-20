using autoplaysharp.Contracts;
using Prism.Commands;
using System;
using System.Windows.Input;

namespace autoplaysharp.App.UI.Tasks.ViewModels
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

        public TaskBaseViewModel(IGame game, IUiRepository repo, ITaskExecutioner taskExecutioner)
        {
            _game = game;
            _repo = repo;
            _taskExecutioner = taskExecutioner;
            AddToQueue = new DelegateCommand(Add);
        }

        private void Add()
        {
            _taskExecutioner.QueueTask(CreateTask());
        }

        protected override IGameTask CreateTask()
        {
            return (Task)Activator.CreateInstance(typeof(Task), _game, _repo);
        }

        public override string Name => typeof(Task).Name;

        public override ICommand AddToQueue { get; }
    }
}
