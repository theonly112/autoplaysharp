using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using autoplaysharp.Contracts;
using Microsoft.Extensions.Logging;

namespace autoplaysharp.Core.Game
{
    /// <summary>
    /// TODO: Threading / Locking ...
    /// </summary>
    public class TaskExecutioner : ITaskExecutioner, ITaskQueue
    {
        private readonly Queue<IGameTask> _queue = new();
        private bool _taskRunning;
        private CancellationTokenSource _source = new();
        private readonly ILogger<TaskExecutioner> _logger;
        private Task _activeTask;

        public IEnumerable<IGameTask> Items => _queue.ToImmutableList();

        public IGameTask ActiveItem { get; private set; }

        public TaskExecutioner(ILogger<TaskExecutioner> logger)
        {
            _logger = logger;
        }

        public void QueueTask(IGameTask task)
        {
            _queue.Enqueue(task);
            RunNext();
        }

        private void RunNext()
        {
            if (!_taskRunning)
            {
                if (_queue.TryDequeue(out var task))
                {
                    _taskRunning = true;
                    ActiveItem = task;
                    _activeTask = Task.Run(() =>
                                           {
                                               _logger.LogDebug("Running task");
                                               return task.Run(_source.Token).ContinueWith(TaskFinished);
                                           });
                }
                else
                {
                    ActiveItem = null;
                    _activeTask = null;
                }
            }

            OnPropertyChanged(nameof(ActiveItem));
            OnPropertyChanged(nameof(Items));
        }

        private async Task TaskFinished(Task t)
        {
            _taskRunning = false;
            _logger.LogDebug("Task finished");

            try
            {
                await t; // unwarp any exception.
            }
            catch (TaskCanceledException)
            {
                _logger.LogInformation("Task was cancelled");
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Task existed with exception. {e}");
            }
            RunNext();
        }

        public async Task CancelActiveTask()
        {
            var active = _activeTask;
            _source.Cancel();
            _source = new CancellationTokenSource();

            try
            {
                await active;
            }
            catch (TaskCanceledException)
            {
                // this is expected.
            }
        }

        public async Task Cancel(IGameTask task)
        {
            if (ActiveItem == task)
            {
                await CancelActiveTask();
            }
            else
            {
                // TODO: is there a better way?
                var items = _queue.Where(x => x != task).ToList();
                _queue.Clear();
                foreach (var item in items)
                {
                    _queue.Enqueue(item);
                }
            }

            OnPropertyChanged(nameof(ActiveItem));
            OnPropertyChanged(nameof(Items));
        }

        public async Task CancelAll()
        {
            await Cancel(ActiveItem);
            _queue.Clear();
            OnPropertyChanged(nameof(ActiveItem));
            OnPropertyChanged(nameof(Items));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
