using autoplaysharp.Contracts;
using autoplaysharp.Game.Tasks;
using ImGuiNET;
using System;

namespace autoplaysharp.Overlay.Windows.Task
{
    internal class TaskToggleButton
    {
        private readonly Func<GameTask> _taskFactory;
        private readonly string _name;
        private readonly ITaskExecutioner _taskExecutioner;
        private bool _running;
        private bool _stopping;

        public TaskToggleButton(Func<GameTask> taskFactory, string name, ITaskExecutioner taskExecutioner)
        {
            _taskFactory = taskFactory;
            _name = name;
            _taskExecutioner = taskExecutioner;
        }

        public void Render()
        {
            if(_stopping)
            {
                ImGui.Button($"Stopping {_name}...");
                return;
            }

            if (!_running)
            {
                if (ImGui.Button(_name))
                {
                    var task = _taskFactory(); ;
                    task.TaskEnded += () => { _running = false; _stopping = false; };
                    _taskExecutioner.QueueTask(task);
                    _running = true;
                }
            }
            else
            {
                if (ImGui.Button($"Stop {_name}"))
                {
                    _taskExecutioner.CancelActiveTask();
                    _stopping = true;
                }
            }
        }
    }

}
