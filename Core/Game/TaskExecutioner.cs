using autoplaysharp.Contracts;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace autoplaysharp.Game
{
    public class TaskExecutioner : ITaskExecutioner, ITaskQueue
    {
        private Queue<IGameTask> _queue = new Queue<IGameTask>();
        private bool _taskRunning = false;
        private CancellationTokenSource _source = new CancellationTokenSource();
        private ILogger<TaskExecutioner> _logger;
        private readonly object _lock = new object();

        public event Action QueueChanged;

        public IEnumerable<IGameTask> Items
        {
            get
            {
                lock (_lock)
                {
                    return _queue.ToList();
                }
            }
        }

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
            lock (_lock)
            {
                if (!_taskRunning)
                {
                    if (_queue.TryDequeue(out var task))
                    {
                        _taskRunning = true;
                        ActiveItem = task;
                        Task.Run(() =>
                        {
                            _logger.LogDebug("Running task");
                            return task.Run(_source.Token).ContinueWith(TaskFinished);
                        });
                    }
                    else
                    {
                        ActiveItem = null;
                    }
                }
            }
            QueueChanged?.Invoke();
        }

        private async void TaskFinished(Task t)
        {
            lock(_lock)
            {
                _taskRunning = false;
                _logger.LogDebug("Task finished");
                RunNext();
            }
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
                _logger.LogError(e, $"Task existed with exception.");
            }
        }

        public void CancelActiveTask()
        {
            _source.Cancel();
            _source = new CancellationTokenSource();
        }

        public void Cancel(IGameTask task)
        {
            lock (_lock)
            {
                if (ActiveItem == task)
                {
                    CancelActiveTask();
                }
                else
                {
                    // TODO: is there a better way?
                    var items = _queue.Where(x => x != task).ToList();
                    _queue.Clear();
                    foreach(var item in items)
                    {
                        _queue.Enqueue(item);
                    }
                }
            }
            QueueChanged?.Invoke();
        }
    }
}
