using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace autoplaysharp.Core.Game.Tasks.Inventory
{
    internal class UpgradeCards : InventoryTask
    {
        public UpgradeCards(IGame game, IUiRepository repository, ISettings settings) : base(game, repository, settings)
        {
        }

        protected override async Task RunCore(CancellationToken token)
        {
            if(!await OpenInventory(token))
            {
                Logger.LogError("Could not open inventory.");
            }

            await ClickWhenVisible(UIds.INVENTORY_TAB_CARDS);
            await ClickWhenVisible(UIds.INVENTORY_TAB_CARDS_UPGRADE_ALL);
            for(int x = 0; x < 3; x++)
            {
              
                var text = Game.GetText(Repository[UIds.INVENTORY_TAB_CARDS_UPGRADE_AMOUNT_DYN, x, 0]);
                int.TryParse(text, out var num);
                if(num > 6)
                {
                    Game.Click(Repository[UIds.INVENTORY_TAB_CARDS_UPGRADE_TYPE_DYN, x, 0]);
                    await ClickWhenVisible(UIds.INVENTORY_TAB_CARDS_UPGRADE_SELECT);
                    await ClickWhenVisible(UIds.INVENTORY_TAB_CARDS_UPGRADE_BUTTON);
                    await ClickWhenVisible(UIds.INVENTORY_TAB_CARDS_UPGRADE_RESULT_OK);
                }
            }
      
            // handles heroic quest notices.
            await GoToMainScreen();
        }
    }
}
