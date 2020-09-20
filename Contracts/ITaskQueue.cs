using System;
using System.Collections.Generic;

namespace autoplaysharp.Contracts
{
    public interface ITaskQueue
    {
        IEnumerable<IGameTask> Items { get; }
        IGameTask ActiveItem { get; }
        event Action QueueChanged;
    }
}
