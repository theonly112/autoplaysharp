using System.Threading;
using System.Threading.Tasks;

namespace autoplaysharp.Contracts
{
    public interface IGameTask
    {
        Task Run(CancellationToken token);
    }
}
