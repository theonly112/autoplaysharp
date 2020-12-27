using System.Collections.Generic;
using System.ComponentModel;

namespace autoplaysharp.Contracts
{
    public interface ITaskQueue : INotifyPropertyChanged
    {
        IEnumerable<IGameTask> Items { get; }
        IGameTask ActiveItem { get; }
    }
}
