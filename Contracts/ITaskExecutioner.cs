using System.Threading.Tasks;

namespace autoplaysharp.Contracts
{
    public interface ITaskExecutioner
    {
        void QueueTask(IGameTask task);

        Task CancelActiveTask();
        Task Cancel(IGameTask task);
        Task CancelAll();
    }
}
