using autoplaysharp.Contracts;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace autoplaysharp.Core.Game.Tasks.Missions.DualEpicQuests
{
    public abstract class GenericDualEpicQuest : GenericEpicQuest
    {
        protected GenericDualEpicQuest(IGame game, IUiRepository repository) : base(game, repository)
        {
        }

        protected abstract string MissionName { get; }
        protected override async Task RunCore(CancellationToken token)
        {
            var status = await StartContentBoardMission(MissionName);
            if (status == null)
            {
                Logger.LogError($"Mission {MissionName} not found on content status board. Stopping...");
                return;
            }

            for (int i = 0; i < status.Available; i++)
            {
                if (!await RunMission())
                {
                    Logger.LogError("Failed to run mission...");
                    break;
                }
            }

            Logger.LogInformation($"Done running {MissionName}");
        }

        private async Task<bool> RunMission()
        {
            if (await StartContentBoardMission(MissionName) == null)
            {
                Logger.LogError($"Cannot start mission {MissionName}");
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


    }
}
