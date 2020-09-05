using autoplaysharp.Contracts;
using autoplaysharp.Core.Game.Tasks;
using autoplaysharp.Core.Game.Tasks.Missions;
using autoplaysharp.Core.Game.Tasks.Missions.DeluxeEpicQuests;
using autoplaysharp.Core.Game.Tasks.Missions.DualEpicQuests;
using autoplaysharp.Game.Tasks;
using autoplaysharp.Game.Tasks.Missions;
using autoplaysharp.Overlay.Windows.Task;
using ImGuiNET;
using System.Collections.Generic;

namespace autoplaysharp.Overlay.Windows
{
    class TaskWindow : IOverlaySubWindow
    {
        private readonly IGame _game;
        private readonly IUiRepository _repository;

        private List<TaskToggleButton> _taskToggleButtons = new List<TaskToggleButton>();

        public TaskWindow(ITaskExecutioner taskExecutioner, IGame game, IUiRepository repository)
        {
            _game = game;
            _repository = repository;

            _taskToggleButtons.Add(new TaskToggleButton(() => new VeiledSecret(_game, _repository), "Veiled Secret", taskExecutioner));
            _taskToggleButtons.Add(new TaskToggleButton(() => new StupidXMen(_game, _repository), "Stupid X-Men", taskExecutioner));
            _taskToggleButtons.Add(new TaskToggleButton(() => new TheBigTwin(_game, _repository), "The Big Twin", taskExecutioner));
            _taskToggleButtons.Add(new TaskToggleButton(() => new TwistedWorld(_game, _repository), "Twisted World", taskExecutioner));
            _taskToggleButtons.Add(new TaskToggleButton(() => new TheFault(_game, _repository), "The Fault", taskExecutioner));

            _taskToggleButtons.Add(new TaskToggleButton(() => new MutualEnemy(_game, _repository), "Mutual Enemy", taskExecutioner));
            _taskToggleButtons.Add(new TaskToggleButton(() => new BeginningOfTheChaos(_game, _repository), "Beginning of the Chaos", taskExecutioner));
            _taskToggleButtons.Add(new TaskToggleButton(() => new DoomsDay(_game, _repository), "Doom's Day", taskExecutioner));
            _taskToggleButtons.Add(new TaskToggleButton(() => new FateOfTheUniverse(_game, _repository), "Fate of the Universe", taskExecutioner));

            _taskToggleButtons.Add(new TaskToggleButton(() => new ShieldLab(_game, _repository), "Shield Lab", taskExecutioner));

            _taskToggleButtons.Add(new TaskToggleButton(() => new AllianceBattle(_game, _repository), "Alliance Battle", taskExecutioner));
            _taskToggleButtons.Add(new TaskToggleButton(() => new DangerRoom(_game, _repository), "Danger Room", taskExecutioner));
            _taskToggleButtons.Add(new TaskToggleButton(() => new TimelineBattle(_game, _repository), "Timeline Battle", taskExecutioner));
            _taskToggleButtons.Add(new TaskToggleButton(() => new CoopMission(_game, _repository), "CO-OP Play", taskExecutioner));
            _taskToggleButtons.Add(new TaskToggleButton(() => new WorldBossInvasion(_game, _repository), "World Boss Invasion", taskExecutioner));
            _taskToggleButtons.Add(new TaskToggleButton(() => new SquadBattle(_game, _repository), "Squad Battle", taskExecutioner));


            _taskToggleButtons.Add(new TaskToggleButton(() => new AutoFight(_game, _repository), "Auto Fight++", taskExecutioner));
        }

        public void Render()
        {
            ShowTasks();
        }

        private void ShowTasks()
        {
            ImGui.Begin("Tasks");
            foreach(var b in _taskToggleButtons)
            {
                b.Render();
            }
            ImGui.End();
        }
    }
}
