using System.Threading;
using System.Threading.Tasks;
using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;
using Microsoft.Extensions.Logging;

namespace autoplaysharp.Core.Game.Tasks.Missions
{
    public class DispatchMission : GameTask
    {
        public DispatchMission(IGame game, IUiRepository repository, ISettings settings) : base(game, repository, settings)
        {
        }

        protected override async Task RunCore(CancellationToken token)
        {
            if (!await GoToMainScreen(token))
            {
                Logger.LogError("Failed to go to the main screen");
                return;
            }
            Game.Click(UIds.MAIN_MENU_ENTER);
            await ClickWhenVisible(UIds.DISPATCH_MISSION_HEADER);
            for (int i = 0; i < 5; i++)
            {
                if (await WaitUntilVisible(UIds.DISPATCH_MISSION_ACQUIRE, token, 2))
                {
                    Game.Click(UIds.DISPATCH_MISSION_ACQUIRE);
                    await CollectAll();
                    break;
                }
                Game.Drag(UIds.MAIN_MENU_SELECT_MISSION_DRAG_LEFT, UIds.MAIN_MENU_SELECT_MISSION_DRAG_RIGHT);
            }

            if (await WaitUntilVisible(UIds.DISPATCH_MISSION_REWARD_ACQUIRED_OK, token))
            {
                Logger.LogDebug("Confirming last message");
                Game.Click(UIds.DISPATCH_MISSION_REWARD_ACQUIRED_OK);
            }
        }

        private async Task CollectAll()
        {
            while (await WaitUntilVisible(UIds.DISPATCH_MISSION_NEXT_REWARD))
            {
                Game.Click(UIds.DISPATCH_MISSION_NEXT_REWARD);
            }
        }
    }
}
