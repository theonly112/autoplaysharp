using autoplaysharp.Contracts;
using autoplaysharp.Game.Tasks;
using autoplaysharp.Game.Tasks.Missions;
using ImGuiNET;
using System;

namespace autoplaysharp.Overlay.Windows
{
    class TaskWindow : IOverlaySubWindow
    {
        private readonly ITaskExecutioner _taskExecutioner;
        private readonly IGame _game;
        private readonly IUiRepository _repository;
        private bool _autoFightActive = false;
        private bool _shieldLabActive;
        private bool _veiledSecretActive;
        private bool _stupidXmen;
        public TaskWindow(ITaskExecutioner taskExecutioner, IGame game, IUiRepository repository)
        {
            _taskExecutioner = taskExecutioner;
            _game = game;
            _repository = repository;
        }

        public void Render()
        {
            ShowTasks();
        }

        private void ShowTasks()
        {
            ImGui.Begin("Tasks");
            AutoFightPlusPlus();
            ShieldLab();
            VeiledSecret();
            StupidXMen();
            ImGui.End();
        }

        private void VeiledSecret()
        {
            if (!_veiledSecretActive)
            {
                if (ImGui.Button("Veiled Secret"))
                {
                    var task = new VeiledSecret(_game, _repository);
                    task.TaskEnded += () => { _veiledSecretActive = false; };
                    _taskExecutioner.QueueTask(task);
                    _veiledSecretActive = true;
                }
            }
            else
            {
                if (ImGui.Button("Veiled Secret"))
                {
                    _taskExecutioner.CancelActiveTask();
                }
            }
        }

        private void StupidXMen()
        {
            if (!_stupidXmen)
            {
                if (ImGui.Button("Stupid X-Men"))
                {
                    var task = new StupidXMen(_game, _repository);
                    task.TaskEnded += () => { _stupidXmen = false; };
                    _taskExecutioner.QueueTask(task);
                    _stupidXmen = true;
                }
            }
            else
            {
                if (ImGui.Button("Stop Stupid X-Men"))
                {
                    _taskExecutioner.CancelActiveTask();
                }
            }
        }

        private void ShieldLab()
        {
            if (!_shieldLabActive)
            {
                if (ImGui.Button("ShieldLab"))
                {
                    var task = new ShieldLab(_game);
                    task.TaskEnded += () => { _shieldLabActive = false; };
                    _taskExecutioner.QueueTask(task);
                    _shieldLabActive = true;
                }
            }
            else
            {
                if (ImGui.Button("Stop ShieldLab"))
                {
                    _taskExecutioner.CancelActiveTask();
                }
            }
        }

        private void AutoFightPlusPlus()
        {
            if (!_autoFightActive)
            {
                if (ImGui.Button("AutoFight++"))
                {
                    var task = new AutoFight(_game);
                    task.TaskEnded += () => { _autoFightActive = false; };
                    _taskExecutioner.QueueTask(task);
                    _autoFightActive = true;
                }
            }
            else
            {
                if (ImGui.Button("Stop AutoFight++"))
                {
                    _taskExecutioner.CancelActiveTask();
                }
            }
        }
    }
}
