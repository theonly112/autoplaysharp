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
using Microsoft.Extensions.DependencyInjection;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;
using System.Threading;

using autoplaysharp.App.UI.Tasks.EpicQuest;
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

        public TasksViewModel(ITaskQueue queue, IGame game, IUiRepository repo, ITaskExecutioner executioner, ISettings settings)
        {
            _queue = queue;
            _game = game;
            _repo = repo;
            _executioner = executioner;
            _settings = settings;
            AddAllToQueue = new DelegateCommand(AddAll, CanExecuteRunRoutine);
            CancelCommand = new DelegateCommand(CancelAll, CanExecuteCancel);
            Add = new DelegateCommand<TaskBaseViewModel>(AddToRoutine, CanAddToRoutine);
            Remove = new DelegateCommand<RoutineViewModel>(RemoveFromRoutine, CanRemoveFromRoutine);
            ExecuteSingleTaskCommand = new DelegateCommand<Type>(ExecuteSingleTask, CanExecuteSingleTask);
            _queue.PropertyChanged += (_, _) => OnQueueChanged();
            OnQueueChanged();
            
            var types = new List<Type>();
            foreach (var referencedAssembly in Assembly.GetEntryAssembly()?.GetReferencedAssemblies()?.Select(Assembly.Load))
            {
                types.AddRange(referencedAssembly.GetTypes()
                    .Where(x =>
                    {
                        return x.IsAssignableTo(typeof(IGameTask)) &&
                               !x.IsAbstract &&
                               x.IsPublic;
                    }));
            }

            if (settings.RoutineItems != null)
            {
                foreach (var routineViewModel in settings.RoutineItems.Where(x => types.Any(t => t.Name == x))
                    .Select(x => new RoutineViewModel(types.FirstOrDefault(t => t.Name == x), game, repo, executioner, settings, queue)))
                {
                    RoutineItems.Add(routineViewModel);
                }
            }
            
            RoutineItems.CollectionChanged += RoutineItems_CollectionChanged;

            if (!RoutineItems.Any())
            {
                foreach (var routineViewModel in types.Where(x => x != typeof(AutoFight))
                    .Select(t => new RoutineViewModel(t, game, repo, executioner, settings, queue)))
                {
                    RoutineItems.Add(routineViewModel);
                }
            }
        }

        private void RoutineItems_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            _settings.RoutineItems = RoutineItems.Select(x => x.Name).ToArray();
        }

        private async void ExecuteSingleTask(Type obj)
        {
            IsEnabled = false;
            var t = (IGameTask)Activator.CreateInstance(obj, _game, _repo, _settings);
            try
            {
                await t.Run(CancellationToken.None);
            }
            catch
            {
                // ignored
            }

            IsEnabled = true;
        }

        private bool CanExecuteSingleTask(Type arg)
        {
            return IsEnabled && ActiveTask == null && !_queue.Items.Any(); ;
        }

        private bool CanRemoveFromRoutine(RoutineViewModel arg)
        {
            return IsEnabled && ActiveTask == null && !_queue.Items.Any(); ;
        }

        private void RemoveFromRoutine(RoutineViewModel obj)
        {
            RoutineItems.Remove(obj);
        }

        private bool CanAddToRoutine(TaskBaseViewModel taskBaseViewModel)
        {
            return IsEnabled && ActiveTask == null && !_queue.Items.Any();
        }

        private void AddToRoutine(TaskBaseViewModel arg)
        {
            RoutineItems.Add(new RoutineViewModel(arg.TaskType, _game, _repo, _executioner, _settings, _queue));
        }

        private bool CanExecuteCancel()
        {
            return _queue.Items.Any();
        }

        private async void CancelAll()
        {
            IsEnabled = false;
            await _executioner.CancelAll();
            IsEnabled = true;
        }

        private bool CanExecuteRunRoutine()
        {
            return !_queue.Items.Any() && _queue.ActiveItem == null;
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
        }

        private static readonly Type[] ViewModelTypes =
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
            typeof(StupidXMenViewModel),
            typeof(TheFaultViewModel),
            typeof(TwistedWorldViewModel),
            typeof(TaskBaseViewModel<TheBigTwin>),
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

        public ObservableCollection<TaskBaseViewModel> Tasks { get; set; } = new(TaskViewModels());

        public ObservableCollection<RoutineViewModel> RoutineItems { get; } = new();

        public bool IsEnabled
        {
            get => _isEnabled;
            set => SetProperty(ref _isEnabled, value);
        }

        public DelegateCommand<TaskBaseViewModel> Add { get; }

        public DelegateCommand<RoutineViewModel> Remove { get; }

        public DelegateCommand<Type> ExecuteSingleTaskCommand { get; set; }

        internal static void ConfigureServices(ServiceCollection serviceCollection)
        {
            foreach (var item in ViewModelTypes)
            {
                serviceCollection.AddSingleton(item);
            }
        }

        private static IEnumerable<TaskBaseViewModel> TaskViewModels()
        {
            return ViewModelTypes.Select(item => (TaskBaseViewModel)App.ServiceProvider.GetService(item));
        }
    }
}
