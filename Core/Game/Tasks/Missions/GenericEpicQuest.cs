using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;
using autoplaysharp.Contracts.Errors;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace autoplaysharp.Core.Game.Tasks.Missions
{
    public abstract class GenericEpicQuest : ContentStatusBoardDependenTask
    {
        protected GenericEpicQuest(IGame game, IUiRepository repository, ISettings settings) : base(game, repository, settings)
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


            if (!await OnGameStart())
            {
                await RunCore(token);
                return true;
            }

            var autoFight = new AutoFight(Game, Repository, Settings, () => Game.IsVisible(UIds.EPIC_QUEST_ENDSCREEN_HOME_BUTTON_IMAGE));
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

            if(!await ClickWhenVisible(UIds.EPIC_QUEST_ENDSCREEN_HOME_BUTTON_IMAGE))
            {
                Logger.LogError("Home button did not appear.");
                return false;
            }

            await Task.Delay(3000, token);
            return true;
        }

        protected async Task UseClearTickets()
        {
            await ClickWhenVisible(UIds.EPIC_QUEST_CLEAR_BUTTON);
            await ClickWhenVisible(UIds.EPIC_QUEST_CLEAR_DIALOG_MIDDLE_BUTTON);
            await ClickWhenVisible(UIds.EPIC_QUEST_CLEAR_RESULT_CLOSE, 10);
        }

        protected virtual Task<bool> OnGameStart()
        {
            return Task.FromResult(true);
        }
    }
}
