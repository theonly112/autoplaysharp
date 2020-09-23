using autoplaysharp.Contracts;
using autoplaysharp.Game.Tasks;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace autoplaysharp.Core.Game.Tasks.Inventory
{
    internal class CombineIso8 : GameTask
    {
        public CombineIso8(IGame game, IUiRepository repository) : base(game, repository)
        {
        }

        protected override async Task RunCore(CancellationToken token)
        {
            await GoToMainScreen();

            Logger.LogError("Not implemented yet");

            // TODO: implement.

            // handles heroic quest notices.
            await GoToMainScreen();
        }
    }
}
