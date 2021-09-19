using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace autoplaysharp.Core.Game.Tasks.Inventory
{
    public class UpgradeCustomGear : InventoryTask
    {
        public UpgradeCustomGear(IGame game, IUiRepository repository, ISettings settings) : base(game, repository, settings)
        {
        }

        protected override async Task RunCore(CancellationToken token)
        {
            if(!await OpenInventory(token))
            {
                return;
            }

            Game.Click(UIds.INVENTORY_TAB_CUSTOM_GEAR);

            if(!await ClickWhenVisible(UIds.INVENTORY_TAB_CUSTOM_GEAR_QUICK_UPGRADE))
            {
                Logger.LogError("Quick Upgrade button not visible.");
                return;
            }

            await Task.Delay(1000, token);
            if (Game.IsVisible(UIds.INVENTORY_TAB_CUSTOM_GEAR_INSUFFICIENT_MATS))
            {
                Game.Click(UIds.INVENTORY_TAB_CUSTOM_GEAR_INSUFFICIENT_MATS_OK);
                Logger.LogError("Insufficient materials for upgrade");
                return;
            }

            if (!await ClickWhenVisible(UIds.INVENTORY_TAB_CUSTOM_GEAR_QUICK_UPGRADE_OK))
            {
                Logger.LogError("Quick Upgrade dialog OK button not visible.");
                return;
            }

            await Task.Delay(1000);

            if(Game.IsVisible(UIds.INVENTORY_TAB_CUSTOM_GEAR_UPGRADED_GEAR_INCLUDED))
            {
                // TODO: we should add a setting if the user doesnt want this.
                Logger.LogDebug("Upgraded gear included.");
                Game.Click(UIds.INVENTORY_TAB_CUSTOM_GEAR_QUICK_UPGRADE_OK);
            }

            // TODO: finde better way to wait until animation has finished.
            await Task.Delay(5000);

            if (!await ClickWhenVisible(UIds.INVENTORY_TAB_CUSTOM_GEAR_QUICK_UPGRADE_RESULT_OK))
            {
                Logger.LogError("Quick Upgrade result OK button not visible.");
                return;
            }

            // handles heroic quest notices.
            await GoToMainScreen();
        }
    }
}
