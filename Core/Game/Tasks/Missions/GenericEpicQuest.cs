using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Errors;
using autoplaysharp.Game.Tasks;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace autoplaysharp.Core.Game.Tasks.Missions
{
    public abstract class GenericEpicQuest : ContentStatusBoardDependenTask
    {
        protected GenericEpicQuest(IGame game, IUiRepository repository) : base(game, repository)
        {
        }

        protected async Task<bool> RunMissionCore(CancellationToken token)
        {
            if (!await WaitUntilVisible(UIds.GENERIC_MISSION_START, token))
            {
                return false;
            }

            await Task.Delay(1000, token);
            Game.Click(UIds.GENERIC_MISSION_START);
            await Task.Delay(1000, token);
            
            if (Game.IsVisible(UIds.GENERIC_MISSION_ITEM_LIMIT_REACHED_NOTICE))
            {
                Game.Click(UIds.GENERIC_MISSION_ITEM_LIMIT_REACHED_NOTICE_OK_BUTTON);
            }

            var autoFight = new AutoFight(Game, Repository, () => Game.IsVisible(UIds.EPIC_QUEST_ENDSCREEN_HOME_BUTTON_IMAGE));
            await autoFight.Run(token);

            if(!await WaitUntilVisible(UIds.EPIC_QUEST_ENDSCREEN_HOME_BUTTON_IMAGE, token, 20, 0.5f))
            {
                Game.OnError(new ElementNotFoundError(Repository[UIds.EPIC_QUEST_ENDSCREEN_HOME_BUTTON_IMAGE]));
                Logger.LogError("Button to navigate to home screen did not appear.");
                return false;
            }

            await Task.Delay(3000, token);

            if (Game.IsVisible(UIds.EPIC_QUEST_ENDSCREEN_NOTICE_ALL_ENTRIES_USED))
            {
                Game.Click(UIds.EPIC_QUEST_ENDSCREEN_NOTICE_ALL_ENTRIES_USED_OK_BUTTON);
                await Task.Delay(1000, token);
            }

            Game.Click(UIds.EPIC_QUEST_ENDSCREEN_HOME_BUTTON_IMAGE);
            await Task.Delay(3000, token);
            return true;
        }
    }
}
