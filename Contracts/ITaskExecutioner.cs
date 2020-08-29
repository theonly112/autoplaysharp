using autoplaysharp.Game.Tasks;
using System;

namespace autoplaysharp.Contracts
{
    interface ITaskExecutioner
    {
        void QueueTask(GameTask task);

        void CancelActiveTask();
    }
}
