﻿using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;
using autoplaysharp.Core.Helper;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace autoplaysharp.Core.Game.Tasks.Missions.DualEpicQuests
{
    public abstract class GenericDualEpicQuest : GenericEpicQuest
    {
        protected GenericDualEpicQuest(IGame game, IUiRepository repository, ISettings settings) : base(game, repository, settings)
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

            if (Settings.EpicQuest.UseClearTickets)
            {
                await ClearDualMission();
            }
            else
            {
                for (int i = 0; i < status.Available; i++)
                {
                    if (!await RunMission(token))
                    {
                        Logger.LogError("Failed to run mission...");
                        break;
                    }
                }
            }

            

            Logger.LogInformation($"Done running {MissionName}");
        }

        private async Task ClearDualMission()
        {
            var status = Game.GetText(UIds.EPIC_QUEST_DUAL_MISSION_LEFT).TryParseStatus();
            if (status.Success)
            {
                if (status.Current > 0)
                {
                    Game.Click(UIds.EPIC_QUEST_DUAL_MISSION_LEFT);
                    await UseClearTickets();
                }
            }

            await StartContentBoardMission(MissionName);

            status = Game.GetText(UIds.EPIC_QUEST_DUAL_MISSION_RIGHT).TryParseStatus();
            if (status.Success)
            {
                if (status.Current > 0)
                {
                    Game.Click(UIds.EPIC_QUEST_DUAL_MISSION_RIGHT);
                    await UseClearTickets();
                }
            }
        }

        protected virtual async Task<bool> RunMission(CancellationToken token)
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
