using autoplaysharp.Contracts;
using autoplaysharp.Game.Tasks;
using System;
using System.Threading.Tasks;

namespace autoplaysharp.Core.Game.Tasks.Missions
{
    public abstract class GenericEpicQuest : ContentStatusBoardDependenTask
    {
        protected GenericEpicQuest(IGame game, IUiRepository repository) : base(game, repository)
        {
        }

        protected async Task<bool> RunMissionCore()
        {
            if (!await WaitUntilVisible("GENERIC_MISSION_START"))
            {
                return false;
            }
            await Task.Delay(1000);
            Game.Click("GENERIC_MISSION_START");
            await Task.Delay(1000);
            if (Game.IsVisible("GENERIC_MISSION_ITEM_LIMIT_REACHED_NOTICE"))
            {
                Game.Click("GENERIC_MISSION_ITEM_LIMIT_REACHED_NOTICE_OK_BUTTON");
            }

            await WaitUntil(() =>
            {
                var missionCompleted = Game.IsVisible("EPIC_QUEST_ENDSCREEN_HOME_BUTTON_IMAGE");
                if (!missionCompleted)
                {
                    Console.WriteLine($"Waiting for mission to be completed");
                }
                return missionCompleted;
            }, 120, 5);

            await Task.Delay(3000);

            if (Game.IsVisible("EPIC_QUEST_ENDSCREEN_NOTICE_ALL_ENTRIES_USED"))
            {
                Game.Click("EPIC_QUEST_ENDSCREEN_NOTICE_ALL_ENTRIES_USED_OK_BUTTON");
                await Task.Delay(1000);
            }

            Game.Click("EPIC_QUEST_ENDSCREEN_HOME_BUTTON_IMAGE");
            await Task.Delay(3000);
            return true;
        }
    }
}
