using autoplaysharp.App.UI.Tasks.ViewModels;
using autoplaysharp.Contracts;
using autoplaysharp.Core.Game.Tasks.Missions;
using autoplaysharp.Core.Game.Tasks.Missions.DeluxeEpicQuests;
using autoplaysharp.Core.Game.Tasks.Missions.DualEpicQuests;
using autoplaysharp.Game.Tasks.Missions;
using Microsoft.Extensions.DependencyInjection;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Data;
using System.Windows.Input;

namespace autoplaysharp.App.UI.Tasks
{
    internal class TasksViewModel : BindableBase
    {
        private object _lock = new object();
        public TasksViewModel(ITaskQueue queue, ITaskExecutioner executioner)
        {
            _queue = queue;
            _executioner = executioner;
            _queue.QueueChanged += OnQueueChanged;
            OnQueueChanged();
            BindingOperations.EnableCollectionSynchronization(Queue, _lock);
        }

        private void OnQueueChanged()
        {
            Queue.Clear();
            foreach (var item in _queue.Items)
            {
                Queue.Add(new TaskQueueItemViewModel(item, _executioner));
            }
            Active = _queue.ActiveItem == null ? null : new TaskQueueItemViewModel(_queue.ActiveItem, _executioner);
            RaisePropertyChanged(nameof(Active));
        }

        public ObservableCollection<TaskQueueItemViewModel> Queue { get; set; } = new ObservableCollection<TaskQueueItemViewModel>();
        public TaskQueueItemViewModel Active { get; set; }

        private static readonly Type[] _viewModelTypes =
        {
            typeof(AllianceBattleViewModel),
            typeof(TaskBaseViewModel<CoopMission>),
            typeof(TaskBaseViewModel<DailyTrivia>),
            typeof(TaskBaseViewModel<DangerRoom>),
            typeof(TaskBaseViewModel<DimensionMission>),
            typeof(TaskBaseViewModel<HeroicQuest>),
            typeof(TaskBaseViewModel<LegendaryBattle>),
            typeof(TaskBaseViewModel<SquadBattle>),
            typeof(TaskBaseViewModel<TimelineBattle>),
            typeof(TaskBaseViewModel<WorldBossInvasion>),

            // Epic Quest
            typeof(TaskBaseViewModel<StupidXMen>),
            typeof(TaskBaseViewModel<TheBigTwin>),
            typeof(TaskBaseViewModel<TheFault>),
            typeof(TaskBaseViewModel<TwistedWorld>),
            typeof(TaskBaseViewModel<VeiledSecret>),

            // Deluxe 
            typeof(TaskBaseViewModel<BeginningOfTheChaos>),
            typeof(TaskBaseViewModel<DoomsDay>),
            typeof(TaskBaseViewModel<FateOfTheUniverse>),
            typeof(TaskBaseViewModel<MutualEnemy>),
        };

        private readonly ITaskQueue _queue;
        private readonly ITaskExecutioner _executioner;

        public ObservableCollection<TaskBaseViewModel> Tasks { get; set; } = new ObservableCollection<TaskBaseViewModel>(TaskViewModels());

        internal static void ConfigureServices(ServiceCollection serviceCollection)
        {
            foreach (var item in _viewModelTypes)
            {
                serviceCollection.AddSingleton(item);
            }
        }

        private static IEnumerable<TaskBaseViewModel> TaskViewModels()
        {
            foreach (var item in _viewModelTypes)
            {
                yield return (TaskBaseViewModel)Wpf.App.ServiceProvider.GetService(item);
            }
        }
    }

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
