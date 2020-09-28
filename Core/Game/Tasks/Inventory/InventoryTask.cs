using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;
using autoplaysharp.Game.Tasks;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace autoplaysharp.Core.Game.Tasks.Inventory
{
    public abstract class InventoryTask : GameTask
    {
        protected InventoryTask(IGame game, IUiRepository repository, ISettings settings) : base(game, repository, settings)
        {
        }

        protected async Task<bool> OpenInventory(CancellationToken token)
        {
            Logger.LogDebug("Opening inventory");
            if (!await GoToMainScreen(token))
            {
                Logger.LogError("Failed to go to main screen");
                return false;
            }

            if (!await OpenMenu())
            {
                Logger.LogError("Failed to open main menu.");
                return false;
            }

            Game.Click(UIds.MAIN_MENU_INVENTORY_BUTTON);

            await Task.Delay(1000, token);
            return true;
        }
    }
}
