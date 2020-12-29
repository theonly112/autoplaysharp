using autoplaysharp.Contracts;
using System;

namespace autoplaysharp.App.UI.Tasks
{
    internal abstract class TaskBaseViewModel
    {
        public abstract string Name { get; }
        public abstract Type TaskType { get; }
    }

    internal class TaskBaseViewModel<TTask> : TaskBaseViewModel where TTask : IGameTask
    {
        public override string Name => typeof(TTask).Name;

        public override Type TaskType => typeof(TTask);
    }
}
