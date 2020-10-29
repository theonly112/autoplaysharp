using autoplaysharp.App.UI.Tasks.AllianceBattle;
using autoplaysharp.App.UI.Tasks.CoopMission;
using autoplaysharp.App.UI.Tasks.DangerRoom;
using autoplaysharp.App.UI.Tasks.DimensionMissions;
using autoplaysharp.App.UI.Tasks.HeroicQuest;
using autoplaysharp.App.UI.Tasks.LegendaryBattle;
using autoplaysharp.App.UI.Tasks.SquadBattle;
using autoplaysharp.App.UI.Tasks.TimelineBattle;
using autoplaysharp.App.UI.Tasks.WorldBoss;
using autoplaysharp.App.UI.Tasks.WorldBossInvasion;
using autoplaysharp.Contracts;
using autoplaysharp.Core.Game.Tasks.Missions;
using autoplaysharp.Core.Game.Tasks.Missions.DeluxeEpicQuests;
using autoplaysharp.Core.Game.Tasks.Missions.DualEpicQuests;
using autoplaysharp.Game.Tasks;
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
            AddAllToQueue = new DelegateCommand(AddAll);
        }

        private void AddAll()
        {
            foreach (var t in Tasks)
            {
                t.AddToQueue.Execute(null);
            }
        }

        public ICommand AddAllToQueue { get; }

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
            typeof(AllianceBattleSettingsViewModel),
            typeof(CoopMissionSettingsViewModel),
            typeof(DangerRoomSettingsViewModel),
            typeof(DimensionMissionSettingsViewModel),
            typeof(HeroicQuestSettingsViewModel),
            typeof(LegendaryBattleSettingsViewModel),
            typeof(SquadBattleSettingsViewModel),
            typeof(TimelineBattleSettingsViewModel),
            typeof(WorldBossInvasionSettingsViewModel),
            typeof(WorldBossSettingsViewModel),
            
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

            // Misc
            typeof(TaskBaseViewModel<DailyTrivia>),
            typeof(TaskBaseViewModel<AutoFight>),
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
}
