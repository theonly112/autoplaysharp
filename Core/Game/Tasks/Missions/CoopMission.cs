using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;
using autoplaysharp.Core.Helper;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace autoplaysharp.Game.Tasks.Missions
{
    /// <summary>
    /// TODOs:
    /// 1. Handle issues during search for team mates. (Disconnect)
    /// 2. Remove the dry waits where possible.
    /// 3. Handle message when all rewards are collected. (COOP_REWARD_NOTICE_DAILY_LIMIT, COOP_REWARD_NOTICE_DAILY_LIMIT_OK)
    /// </summary>
    public class CoopMission : ContentStatusBoardDependenTask
    {
        public CoopMission(IGame game, IUiRepository repository, ISettings settings) : base(game, repository, settings)
        {
        }

        public int RewardCount { get; set; }

        protected override async Task RunCore(CancellationToken token)
        {
            if (await StartContentBoardMission("CO-OP PLAY") == null)
            {
                return;
            }

            await Task.Delay(2000);
            await RunCoopMissions(token);
            await GoToMainScreen();
        }

        private async Task RunCoopMissions(CancellationToken token)
        {
            while (true)
            {
                var text = Game.GetText(UIds.COOP_REWARD_COUNT);
                var status = text.TryParseStatus();
                if (!status.Success)
                {
                    Logger.LogError("Could not get avilable reward count");
                    return;
                }

                if (status.Current == 0)
                {
                    Logger.LogError("Done with co-op. No rewards available.");
                    return;
                }

                if (Game.IsVisible(UIds.COOP_REWARD_ACQUIRED))
                {
                    Game.Click(UIds.COOP_REWARD_ACQUIRED);
                    await Task.Delay(5000);
                    Game.Click(UIds.COOP_REWARD_ACQUIRE_REWARD);
                    await Task.Delay(5000);
                    Game.Click(UIds.COOP_REWARD_ACQUIRE_REWARD_OK);
                    await Task.Delay(5000);
                    RewardCount--;
                    if(RewardCount == 0)
                    {
                        break;
                    }
                }

                if (Game.IsVisible(UIds.COOP_AUTO_REPEAT_IMAGE))
                {
                    Game.Click(UIds.COOP_AUTO_REPEAT_IMAGE);
                }

                if (!Game.IsVisible(UIds.COOP_START))
                {
                    Game.Click(UIds.COOP_DEPLOY_CHARACTER);
                }

                if (!await WaitUntilVisible(UIds.COOP_START, token))
                {
                    Logger.LogError("Start button not available.");
                    return;
                }
                Game.Click(UIds.COOP_START);

                if (!await HandleStartNotices())
                {
                    Logger.LogError("Unable to handle mission start notice.");
                    return;
                }

                await WaitForMissionEnd(token);

                await Task.Delay(5000);

                Game.Click(UIds.COOP_ENDSCREEN_NEXT_BUTTON);

                await Task.Delay(5000);
            }
        }

        protected async Task WaitForMissionEnd(CancellationToken token)
        {
            if (!await WaitUntilVisible(UIds.COOP_ENDSCREEN_MISSION_SUCCESS, token, 60, 1))
            {
                if (Game.IsVisible(UIds.COOP_REWARD_NOTICE_DAILY_LIMIT))
                {
                    Game.Click(UIds.COOP_REWARD_NOTICE_DAILY_LIMIT_OK);
                    await Task.Delay(2000);
                    Logger.LogInformation("Finished CO-OP missions.");
                    return;
                }

                Logger.LogDebug("Game did not start");

                if(Game.IsVisible(UIds.COOP_WAITING_FOR_OTHER_PLAYERS_CANCEL))
                {
                    Game.Click(UIds.COOP_WAITING_FOR_OTHER_PLAYERS_CANCEL);
                    await Task.Delay(1000);
                    Game.Click(UIds.COOP_START);
                    await WaitForMissionEnd(token);
                }

                return;
            }
        }
    }
}
