using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;
using autoplaysharp.Contracts.Errors;
using autoplaysharp.Game.Tasks;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace autoplaysharp.Core.Game.Tasks.Inventory
{
    internal class CombineUru : InventoryTask
    {
        public CombineUru(IGame game, IUiRepository repository, ISettings settings) : base(game, repository, settings)
        {
        }

        protected override async Task RunCore(CancellationToken token)
        {
            if (!Game.IsVisible(UIds.INVENTORY_TAB_MATERIAL))
            {
                if(!await OpenInventory(token))
                {
                    return;
                }
            }

            Logger.LogDebug("Going to material tab.");
            Game.Click(UIds.INVENTORY_TAB_MATERIAL);
            await FindUru();

            Game.Click(UIds.INVENTORY_TAB_MATERIAL_URU_COMBINE_X_TIMES_BUTTON);
            await Task.Delay(1000);

            Logger.LogDebug("Skipping combine animations.");
            await ClickWhenVisible(UIds.INVENTORY_TAB_MATERIAL_URU_COMBINE_SKIP);
            await Task.Delay(1000);


            Logger.LogDebug("Accepting combine result");
            var acceptedText = new[] { "OK", "0K", "oK" }; // Doesnt reliably dected OK.
            if(!await WaitUntil(() => acceptedText.Any(x => Game.GetText(UIds.INVENTORY_TAB_MATERIAL_URU_COMBINE_OK) == x)))
            {
                Logger.LogError("Failed to accept combine result");
                Game.OnError(new ElementNotFoundError(Repository[UIds.INVENTORY_TAB_MATERIAL_URU_COMBINE_OK]));
                return;
            }
            else
            {
                Game.Click(UIds.INVENTORY_TAB_MATERIAL_URU_COMBINE_OK);
            }

            await Task.Delay(1000);

            Logger.LogDebug("Closing combine dialog");
            if(!await ClickWhenVisible(UIds.INVENTORY_TAB_MATERIAL_URU_COMBINE_ALL_CANCEL))
            {
                Logger.LogError("Failed to close combine dialog.");
                Game.OnError(new ElementNotFoundError(Repository[UIds.INVENTORY_TAB_MATERIAL_URU_COMBINE_ALL_CANCEL]));
                return;
            }

            await Task.Delay(1000);
            await HandleHeroicQuestNotice();

            if(!await GoToMainScreen())
            {
                Logger.LogError("Failed to go back to main screen.");
            }
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
