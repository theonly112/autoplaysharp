using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace autoplaysharp.Core.Game.Tasks.Inventory
{
    internal class EnhanceIso8 : GameTask
    {
        public EnhanceIso8(IGame game, IUiRepository repository, ISettings settings) : base(game, repository, settings)
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
