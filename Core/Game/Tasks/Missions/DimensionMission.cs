using autoplaysharp.Contracts;
using autoplaysharp.Core.Helper;
using autoplaysharp.Game.Tasks;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace autoplaysharp.Core.Game.Tasks.Missions
{
    public class DimensionMission : ContentStatusBoardDependenTask
    {
        public DimensionMission(IGame game, IUiRepository repository) : base(game, repository)
        {
        }

        /*
         * TODO: allow settings:
         * Use clear tickets?
         */

        public int CollectRewardCount { get; set; } = 5;

        protected override async Task RunCore(CancellationToken token)
        {
            var missionStatus = await StartContentBoardMission("DIMENSION MISSION");
            if (missionStatus == null)
            {
                Logger.LogError("Failed to start Dimension Mission");
                return;
            }

            if(missionStatus.Available == 0)
            {
                Logger.LogInformation("Already collected all rewards.");
                return;
            }

            if(CollectRewardCount > missionStatus.Available)
            {
                Logger.LogError($"Cannot collect {CollectRewardCount}. Only {missionStatus.Available} left.");
            }

            await Task.Delay(1000);

            if(!await CollectRewards())
            {
                Logger.LogError("Collection rewards failed");
                return;
            }

            if (CollectRewardCount == 0)
            {
                Logger.LogInformation("Collected required rewards. Stopping");
                return;
            }

            while (CollectRewardCount > 0)
            {
                await RunDimensionMissions();

                await CollectRewards();
            }
        }

        private async Task RunDimensionMissions()
        {
            Game.Click(UIds.DIMENSION_MISSION_READY_BUTTON);

            await ClickWhenVisible(UIds.DIMENSION_MISSION_CLEAR_BUTTON);

            await Task.Delay(1000);

            if (Game.IsVisible(UIds.DIMENSION_MISSION_NOTICE_USE_HIDDEN_TICKET))
            {
                Game.Click(UIds.DIMENSION_MISSION_NOTICE_USE_HIDDEN_TICKET_DONT_USE);
            }

            await Task.Delay(1000);

            Game.Click(UIds.DIMENSION_MISSION_USE_1_CLEAR_TICKET_BUTTON);

            await Task.Delay(5000);

            var statusText = Game.GetText(UIds.DIMENSION_MISSION_CLEAR_TICKET_ENDSCREEN_STATUS);
            var rewardStatus = statusText.TryParseStatus();

            while (rewardStatus.Current < rewardStatus.Max)
            {
                Logger.LogDebug("Running 1x Dimension Mission.");
                Logger.LogDebug($"Status is: {rewardStatus}");
                Game.Click(UIds.DIMENSION_MISSION_CLEAR_TICKET_ENDSCREEN_USE_1_TICKET);

                // TODO: handle inventory full?

                await Task.Delay(5000);
                statusText = Game.GetText(UIds.DIMENSION_MISSION_CLEAR_TICKET_ENDSCREEN_STATUS);
                rewardStatus = statusText.TryParseStatus();
            }

            Game.Click(UIds.DIMENSION_MISSION_CLEAR_TICKET_ENDSCREEN_CLOSE);
            await Task.Delay(2000);

            Game.Click(UIds.DIMENSION_MISSION_BACK_BUTTON);
            await Task.Delay(1000);
        }

        private async Task<bool> CollectRewards()
        {
            var statusText = Game.GetText(UIds.DIMENSION_MISSION_REWARD_STATUS);
            var rewardStatus = statusText.TryParseStatus();
            while (rewardStatus.Current >= rewardStatus.Max && CollectRewardCount > 0)
            {
                Game.Click(UIds.DIMENSION_MISSION_REWARD_COLLECT_BUTTON);
                await Task.Delay(2000);

                if (!await WaitUntilVisible(UIds.DIMENSION_MISSION_REWARD_ACQUIRED_NOTICE))
                {
                    Logger.LogError("Reward notice did not show up");
                    return false;
                }
                else
                {
                    CollectRewardCount--;
                    Logger.LogInformation($"Collected Dimension Mission reward. Left to do: {CollectRewardCount}");
                    Game.Click(UIds.DIMENSION_MISSION_REWARD_ACQUIRED_NOTICE_OK);
                    await Task.Delay(1000);
                    await HandleHeroicQuestNotice();
                }

                statusText = Game.GetText(UIds.DIMENSION_MISSION_REWARD_STATUS);
                rewardStatus = statusText.TryParseStatus();
            }
            return true;
        }
    }
}
