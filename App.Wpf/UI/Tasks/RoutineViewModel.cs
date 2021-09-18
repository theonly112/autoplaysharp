using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;

namespace autoplaysharp.App.UI.Tasks
{
    internal class RoutineViewModel : INotifyPropertyChanged
    {
        private readonly IGame _game;
        private readonly IUiRepository _repo;
        private readonly ITaskExecutioner _taskExecutioner;
        private readonly ISettings _settings;
        private readonly ITaskQueue _queue;
        private IGameTask _task;
        private bool _isActive;
        private bool _isChecked = true;

        public RoutineViewModel(Type taskType, IGame game, IUiRepository repo, 
                                ITaskExecutioner taskExecutioner, ISettings settings, ITaskQueue queue)
        {
            TaskType = taskType;
            _game = game;
            _repo = repo;
            _taskExecutioner = taskExecutioner;
            _settings = settings;
            _queue = queue;
            _queue.PropertyChanged += QueueOnPropertyChanged;
        }

        private void QueueOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            IsActive = _queue.ActiveItem == _task;
        }

        public string Name => TaskType.Name;

        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                _isChecked = value;
                OnPropertyChanged();
            }
        }

        public Type TaskType { get; }

        public bool IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                OnPropertyChanged();
            }
        }

        public void AddToQueue()
        {
            _taskExecutioner.QueueTask(CreateTask());
        }

        private IGameTask CreateTask()
        {
            _task = (IGameTask)Activator.CreateInstance(TaskType, _game, _repo, _settings);
            return _task;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}