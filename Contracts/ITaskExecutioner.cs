namespace autoplaysharp.Contracts
{
    public interface ITaskExecutioner
    {
        void QueueTask(IGameTask task);

        void CancelActiveTask();
        void Cancel(IGameTask task);
    }
}
