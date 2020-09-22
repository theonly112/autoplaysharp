using autoplaysharp.Contracts;
using autoplaysharp.Game.Tasks;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace autoplaysharp.Core.Game.Tasks.Inventory
{
    internal class UpgradeUru : GameTask
    {
        public UpgradeUru(IGame game, IUiRepository repository) : base(game, repository)
        {
        }

        protected override async Task RunCore(CancellationToken token)
        {
            if (!Game.IsVisible(UIds.INVENTORY_TAB_MATERIAL))
            {
                Logger.LogDebug("Opening inventory");
                if (!await GoToMainScreen(token))
                {
                    Logger.LogError("Failed to go to main screen");
                    return;
                }

                if (!await OpenMenu())
                {
                    Logger.LogError("Failed to open main menu.");
                    return;
                }

                Game.Click(UIds.MAIN_MENU_INVENTORY_BUTTON);

                await Task.Delay(1000, token);
            }

            Logger.LogDebug("Going to material tab.");
            Game.Click(UIds.INVENTORY_TAB_MATERIAL);
            await FindUru();

            Game.Click(UIds.INVENTORY_TAB_MATERIAL_URU_COMBINE_X_TIMES_BUTTON);
            await Task.Delay(1000);

            if (Game.IsVisible(UIds.INVENTORY_TAB_MATERIAL_URU_COMBINE_SKIP))
            {
                Logger.LogDebug("Skipping combine animations.");
                Game.Click(UIds.INVENTORY_TAB_MATERIAL_URU_COMBINE_SKIP);
                await Task.Delay(1000);
            }

            // TODO: somewhere here there should be a notice regarding heroic quests.

            Logger.LogDebug("Accepting combine result");
            Game.Click(UIds.INVENTORY_TAB_MATERIAL_URU_COMBINE_OK);

            await Task.Delay(1000);

            Logger.LogDebug("Closing combine dialog");
            Game.Click(UIds.INVENTORY_TAB_MATERIAL_URU_COMBINE_ALL_CANCEL);

            await Task.Delay(1000);
        }

        private async Task FindUru()
        {
            for (int x = 0; x < 6; x++)
            {
                for (int y = 0; y < 3; y++)
                {
                    Game.Click(Repository[UIds.INVENTORY_TAB_MATERIAL_ITEM_GRID, x, y]);
                    await Task.Delay(100);
                    if (Game.IsVisible(UIds.INVENTORY_TAB_MATERIAL_URU_COMBINE_ALL))
                    {
                        Game.Click(UIds.INVENTORY_TAB_MATERIAL_URU_COMBINE_ALL);
                        await Task.Delay(1000);
                        return;
                    }
                }
            }
        }
    }
}
