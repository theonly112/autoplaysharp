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
            await UpdateContentStatusBoard();
            var status = GetMissionStatus(MissionName);
            if (status == null)
            {
                Console.WriteLine($"Mission {MissionName} not found on content status board. Stopping...");
                return;
            }

            for (int i = 0; i < status.Available; i++)
            {
                await RunMission();
            }

            Console.WriteLine($"Done running {MissionName}");
        }

        private async Task RunMission()
        {
            await StartContentBoardMission(MissionName);
            await Task.Delay(1000);
            var text = Game.GetText("EPIC_QUEST_DUAL_MISSION_LEFT");
            var statusMatch = ContentStatus.StatusRegex.Match(text);
            if (int.TryParse(statusMatch.Groups[1].Value, out var num))
            {
                if (num > 0)
                {
                    Game.Click("EPIC_QUEST_DUAL_MISSION_LEFT");
                    await RunMissionCore();
                }
            }

            text = Game.GetText("EPIC_QUEST_DUAL_MISSION_RIGHT");
            statusMatch = ContentStatus.StatusRegex.Match(text);
            if (!int.TryParse(statusMatch.Groups[2].Value, out num))
            {
                if (num > 0)
                {
                    Game.Click("EPIC_QUEST_DUAL_MISSION_RIGHT");
                    await RunMissionCore();
                }
            }
        }

        private async Task RunMissionCore()
        {
            await WaitUntilVisible("GENERIC_MISSION_START");
            Game.Click("GENERIC_MISSION_START");
            await Task.Delay(1000);
            if (Game.IsVisible("GENERIC_MISSION_ITEM_LIMIT_REACHED_NOTICE"))
            {
                Game.Click("GENERIC_MISSION_ITEM_LIMIT_REACHED_NOTICE_OK_BUTTON");
            }

            await WaitUntil(() =>
            {
                // TODO: doesnt work for stupid x-men... need to find other solution.
                // Guess we have to wait for "MISSION SUCCESS" text.
                var missionCompleted = Game.GetText("EPIC_QUEST_ENDSCREEN_MISION_NAME") == MissionName;
                if (!missionCompleted)
                {
                    Console.WriteLine($"Waiting for mission {MissionName} to be completed");
                }
                return missionCompleted;
            }, 120, 1);

            Game.Click("EPIC_QUEST_ENDSCREEN_HOME_BUTTON");
            await Task.Delay(1000);
        }
    }
}
