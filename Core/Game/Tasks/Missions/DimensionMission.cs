using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;
using autoplaysharp.Core.Helper;
using autoplaysharp.Game.Tasks;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using autoplaysharp.Contracts.Errors;

namespace autoplaysharp.Core.Game.Tasks.Missions
{
    public class DimensionMission : ContentStatusBoardDependenTask
    {
        public DimensionMission(IGame game, IUiRepository repository, ISettings settings) : base(game, repository, settings)
        {
            CollectRewardCount = settings.DimensionMission.RewardsToCollect;
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

            await Task.Delay(2000);

            if(Game.IsVisible(UIds.DIMENSION_MISSION_REWARD_CLOSE_AD))
            {
                Game.Click(UIds.DIMENSION_MISSION_REWARD_CLOSE_AD);
                await Task.Delay(1500);
                Game.Click(UIds.DIMENSION_MISSION_REWARD_CLOSE_AD_NOTICE_OK);
            }

            if(!await CollectRewards(token))
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

                await CollectRewards(token);
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
            await HandleStartNotices();


            var statusText = Game.GetText(UIds.DIMENSION_MISSION_CLEAR_TICKET_ENDSCREEN_STATUS);
            var rewardStatus = statusText.TryParseStatus();

            while (rewardStatus.Current < rewardStatus.Max)
            {
                Logger.LogDebug("Running 1x Dimension Mission.");
                Logger.LogDebug($"Status is: {rewardStatus}");
                Game.Click(UIds.DIMENSION_MISSION_CLEAR_TICKET_ENDSCREEN_USE_1_TICKET);

                await Task.Delay(5000);
                await HandleStartNotices();

                statusText = Game.GetText(UIds.DIMENSION_MISSION_CLEAR_TICKET_ENDSCREEN_STATUS);
                rewardStatus = statusText.TryParseStatus();
            }

            Game.Click(UIds.DIMENSION_MISSION_CLEAR_TICKET_ENDSCREEN_CLOSE);
            await Task.Delay(2000);

            await HandleHeroicQuestNotice(1);

            await Task.Delay(1000);

            if (!await ClickWhenVisible(UIds.DIMENSION_MISSION_BACK_BUTTON))
            {
                Game.OnError(new ElementNotFoundError(Repository[UIds.DIMENSION_MISSION_BACK_BUTTON]));
            }
            await Task.Delay(1000);
        }

        private async Task<bool> CollectRewards(CancellationToken token)
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
                    // If multiple rewards are available there is another notice.
                    if(Game.IsVisible(UIds.DIMENSION_MISSION_REWARD_ACQUIRED_NOTICE_CLOSE))
                    {
                        Game.Click(UIds.DIMENSION_MISSION_REWARD_ACQUIRED_NOTICE_OK);
                    }
                    else
                    {
                        Game.Click(UIds.DIMENSION_MISSION_REWARD_ACQUIRED_NOTICE_OK);
                    }
                    await Task.Delay(1000);
                    if(await HandleHeroicQuestNotice())
                    {
                        // Restart because we exit
                        await Run(token);
                        return true;
                    }
                }

                statusText = Game.GetText(UIds.DIMENSION_MISSION_REWARD_STATUS);
                rewardStatus = statusText.TryParseStatus();
            }
            return true;
        }
    }
}
