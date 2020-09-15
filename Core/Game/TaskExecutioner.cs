using autoplaysharp.Contracts;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace autoplaysharp.Game
{
    public class TaskExecutioner : ITaskExecutioner
    {
        private Queue<IGameTask> _queue = new Queue<IGameTask>();
        private bool _taskRunning = false;
        private CancellationTokenSource _source = new CancellationTokenSource();
        private ILogger<TaskExecutioner> _logger;
        private readonly object _lock = new object();

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
                        Task.Run(() =>
                        {
                            _logger.LogDebug("Running task");
                            return task.Run(_source.Token).ContinueWith(TaskFinished);
                        });
                    }
                }
            }
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
    }
}
