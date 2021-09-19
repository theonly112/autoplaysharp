using autoplaysharp.Contracts;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using autoplaysharp.Contracts.Configuration;
using autoplaysharp.Core.Game.Tasks;

namespace autoplaysharp.App.UI.Tasks
{
    internal class TasksViewModel : BindableBase
    {
        private readonly ITaskQueue _queue;
        private readonly IGame _game;
        private readonly IUiRepository _repo;
        private readonly ITaskExecutioner _executioner;
        private readonly ISettings _settings;
        private bool _isEnabled = true;
        private IGameTask _activeTask;
        private bool _canChangeSettings;

        public TasksViewModel(ITaskQueue queue, IGame game, IUiRepository repo, ITaskExecutioner executioner, ISettings settings)
        {
            _queue = queue;
            _game = game;
            _repo = repo;
            _executioner = executioner;
            _settings = settings;
            AddAllToQueue = new DelegateCommand(AddAll, CanExecuteRunRoutine);
            CancelCommand = new DelegateCommand(CancelAll, CanExecuteCancel);
            Add = new DelegateCommand<Type>(AddToRoutine, CanAddToRoutine);
            Remove = new DelegateCommand<RoutineViewModel>(RemoveFromRoutine, CanRemoveFromRoutine);
            _queue.PropertyChanged += (_, _) => OnQueueChanged();
            OnQueueChanged();
            
            var types = new List<Type>();

            var entryAssembly = Assembly.GetEntryAssembly();
            var referencedAssemblies = entryAssembly?.GetReferencedAssemblies();
            if (referencedAssemblies == null)
            {
                throw new Exception();
            }

            foreach (var referencedAssembly in referencedAssemblies)
            {
                var assembly = Assembly.Load(referencedAssembly);
                types.AddRange(assembly.GetTypes()
                    .Where(x => x.IsAssignableTo(typeof(IGameTask)) &&
                                !x.IsAbstract &&
                                x.IsPublic));
            }

            foreach (var type in types)
            {
                Tasks.Add(type);
            }

            if (settings.RoutineItems != null)
            {
                foreach (var routineViewModel in settings.RoutineItems.Where(x => types.Any(t => t.Name == x))
                    .Select(x => new RoutineViewModel(types.FirstOrDefault(t => t.Name == x), game, repo, executioner, settings, queue)))
                {
                    routineViewModel.PropertyChanged += RoutineViewModelOnPropertyChanged;
                    RoutineItems.Add(routineViewModel);
                }
            }
            
            RoutineItems.CollectionChanged += RoutineItems_CollectionChanged;

            if (!RoutineItems.Any())
            {
                foreach (var routineViewModel in types.Where(x => x != typeof(AutoFight))
                    .Select(t => new RoutineViewModel(t, game, repo, executioner, settings, queue)))
                {
                    routineViewModel.PropertyChanged += RoutineViewModelOnPropertyChanged;
                    RoutineItems.Add(routineViewModel);
                }
            }

            AddAllToQueue.RaiseCanExecuteChanged();
        }

        private void RoutineViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RoutineViewModel.IsChecked))
            {
                RaisePropertyChanged(nameof(CheckAll));
                AddAllToQueue.RaiseCanExecuteChanged();
            }
        }

        private void RoutineItems_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            _settings.RoutineItems = RoutineItems.Select(x => x.Name).ToArray();
        }

        private bool CanRemoveFromRoutine(RoutineViewModel arg)
        {
            return IsEnabled && ActiveTask == null && !_queue.Items.Any();
        }

        private void RemoveFromRoutine(RoutineViewModel obj)
        {
            RoutineItems.Remove(obj);
        }

        private bool CanAddToRoutine(Type _)
        {
            return IsEnabled && ActiveTask == null && !_queue.Items.Any();
        }

        private void AddToRoutine(Type taskType)
        {
            var routineViewModel = new RoutineViewModel(taskType, _game, _repo, _executioner, _settings, _queue);
            routineViewModel.PropertyChanged += RoutineViewModelOnPropertyChanged;
            RoutineItems.Add(routineViewModel);
            AddAllToQueue.RaiseCanExecuteChanged();
        }

        private bool CanExecuteCancel()
        {
            return _queue.Items.Any() || _queue.ActiveItem != null;
        }

        private async void CancelAll()
        {
            IsEnabled = false;
            await _executioner.CancelAll();
            IsEnabled = true;
        }

        private bool CanExecuteRunRoutine()
        {
            return !_queue.Items.Any() && _queue.ActiveItem == null && RoutineItems.Any(i => i.IsChecked);
        }

        private void AddAll()
        {
            foreach (var t in RoutineItems.Where(x => x.IsChecked))
            {
                t.AddToQueue();
            }

            AddAllToQueue.RaiseCanExecuteChanged();
        }

        public DelegateCommand AddAllToQueue { get; }
        public DelegateCommand CancelCommand { get; }

        public IGameTask ActiveTask
        {
            get => _activeTask;
            set => SetProperty(ref  _activeTask, value);
        }

        private void OnQueueChanged()
        {
            ActiveTask = _queue.ActiveItem;

            CancelCommand.RaiseCanExecuteChanged();
            AddAllToQueue.RaiseCanExecuteChanged();
            Remove.RaiseCanExecuteChanged();
            Add.RaiseCanExecuteChanged();
            CanChangeSettings = IsEnabled && ActiveTask == null && !_queue.Items.Any();
        }

        public ObservableCollection<RoutineViewModel> RoutineItems { get; } = new();

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        public bool CanChangeSettings
        {
            get => _canChangeSettings;
            set => SetProperty(ref _canChangeSettings, value);
        }

        public DelegateCommand<Type> Add { get; }

        public DelegateCommand<RoutineViewModel> Remove { get; }

        public ObservableCollection<Type> Tasks { get; } = new();

        public bool? CheckAll
        {
            get
            {
                if (RoutineItems.Select(item => item.IsChecked).Distinct().Count() == 2)
                {
                    return null;
                }
                return RoutineItems.All(item => item.IsChecked);
            }
            set
            {
                foreach (var routineViewModel in RoutineItems)
                {
                    routineViewModel.IsChecked = value.GetValueOrDefault();
                }
            }
        }
    }
}
