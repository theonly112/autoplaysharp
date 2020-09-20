using autoplaysharp.Contracts;
using autoplaysharp.Core.Helper;
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
                if (!await RunMission(token))
                {
                    Logger.LogError("Failed to run mission...");
                    break;
                }
            }

            Logger.LogInformation($"Done running {MissionName}");
        }

        private async Task<bool> RunMission(CancellationToken token)
        {
            if (await StartContentBoardMission(MissionName) == null)
            {
                Logger.LogError($"Cannot start mission {MissionName}");
                return false;
            }

            await WaitUntil(() => { return Game.GetText(UIds.EPIC_QUEST_DUAL_MISSION_LEFT).Contains("/"); }, token);
            await Task.Delay(1000, token);

            var status = Game.GetText(UIds.EPIC_QUEST_DUAL_MISSION_LEFT).TryParseStatus();
            if (status.Success)
            {
                if (status.Current > 0)
                {
                    Game.Click(UIds.EPIC_QUEST_DUAL_MISSION_LEFT);
                    return await RunMissionCore(token);
                }
            }

            status = Game.GetText(UIds.EPIC_QUEST_DUAL_MISSION_RIGHT).TryParseStatus();
            if (status.Success)
            {
                if (status.Current > 0)
                {
                    Game.Click(UIds.EPIC_QUEST_DUAL_MISSION_RIGHT);
                    return await RunMissionCore(token);
                }
            }

            return true;
        }


    }
}
