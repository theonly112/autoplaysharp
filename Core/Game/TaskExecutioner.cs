﻿using autoplaysharp.Contracts;
using autoplaysharp.Game.Tasks;
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
        private readonly object _lock = new object();

        public void QueueTask(IGameTask task)
        {
            _queue.Enqueue(task);
        }

        public void Update()
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
                            Console.WriteLine("Running task");
                            return task.Run(_source.Token).ContinueWith(TaskFinished);
                        });
                    }
                }
            }

        }

        private void TaskFinished(Task t)
        {
            lock(_lock)
            {
                _taskRunning = false;
                Console.WriteLine("Task finished");
            }
        }

        public void CancelActiveTask()
        {
            _source.Cancel();
            _source = new CancellationTokenSource();
        }
    }
}