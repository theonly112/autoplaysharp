using autoplaysharp.Contracts;
using autoplaysharp.Game.Tasks;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace autoplaysharp.Core.Game.Tasks.Inventory
{
    internal class EnhanceIso8 : GameTask
    {
        public EnhanceIso8(IGame game, IUiRepository repository) : base(game, repository)
        {
        }

        protected override Task RunCore(CancellationToken token)
        {
            // TODO: implement this...
            Logger.LogError("Enhance not implemented yet.");
            return Task.CompletedTask;
        }
    }
}
