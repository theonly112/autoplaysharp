using autoplaysharp.Contracts;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace autoplaysharp.Game.Tasks.Missions
{
    abstract class GenericDualEpicQuest : ContentStatusBoardDependenTask
    {
        protected GenericDualEpicQuest(IGame game, IUiRepository repository) : base(game, repository)
        {
        }

        protected abstract string MissionName { get; }
        protected override async Task RunCore(CancellationToken token)
        {
            if(!await UpdateContentStatusBoard())
            {
                Console.WriteLine("Failed to update content status board");
                return;
            }

            var status = GetMissionStatus(MissionName);
            if (status == null)
            {
                Console.WriteLine($"Mission {MissionName} not found on content status board. Stopping...");
                return;
            }

            for (int i = 0; i < status.Available; i++)
            {
                if(!await RunMission())
                {
                    Console.WriteLine("Failed to run mission...");
                    break;
                }
            }

            Console.WriteLine($"Done running {MissionName}");
        }

        private async Task<bool> RunMission()
        {
            if(!await StartContentBoardMission(MissionName))
            {
                Console.WriteLine($"Cannot start mission {MissionName}");
                return false;
            }

            await WaitUntil(() => { return Game.GetText("EPIC_QUEST_DUAL_MISSION_LEFT").Contains("/"); });
            await Task.Delay(1000);

            var text = Game.GetText("EPIC_QUEST_DUAL_MISSION_LEFT");
            var statusMatch = ContentStatus.StatusRegex.Match(text);
            if (int.TryParse(statusMatch.Groups[1].Value, out var num))
            {
                if (num > 0)
                {
                    Game.Click("EPIC_QUEST_DUAL_MISSION_LEFT");
                    return await RunMissionCore();
                }
            }

            text = Game.GetText("EPIC_QUEST_DUAL_MISSION_RIGHT");
            statusMatch = ContentStatus.StatusRegex.Match(text);
            if (int.TryParse(statusMatch.Groups[1].Value, out num))
            {
                if (num > 0)
                {
                    Game.Click("EPIC_QUEST_DUAL_MISSION_RIGHT");
                    return await RunMissionCore();
                }
            }

            return true;
        }

        private async Task<bool> RunMissionCore()
        {
            if(!await WaitUntilVisible("GENERIC_MISSION_START"))
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
                    Console.WriteLine($"Waiting for mission {MissionName} to be completed");
                }
                return missionCompleted;
            }, 120, 1);

            await Task.Delay(3000);

            if(Game.IsVisible("EPIC_QUEST_ENDSCREEN_NOTICE_ALL_ENTRIES_USED"))
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
