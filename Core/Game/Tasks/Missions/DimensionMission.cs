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
         * Collect specific amount of rewards.
         */

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

            await Task.Delay(1000);

            var statusText = Game.GetText(UIds.DIMENSION_MISSION_REWARD_STATUS);
            var rewardStatus = statusText.TryParseStatus();
            if(!rewardStatus.Success)
            {
                Logger.LogError($"Failed to parse reward status: {statusText}");
                return;
            }

            // TODO: Check if reward availble?

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

            statusText = Game.GetText(UIds.DIMENSION_MISSION_CLEAR_TICKET_ENDSCREEN_STATUS);
            rewardStatus = statusText.TryParseStatus();

            while(rewardStatus.Current < rewardStatus.Max)
            {
                Logger.LogDebug("Running 1x Dimension Mission");
                Logger.LogDebug($"Status is: {rewardStatus}");
                Game.Click(UIds.DIMENSION_MISSION_CLEAR_TICKET_ENDSCREEN_USE_1_TICKET);
                await Task.Delay(5000);
                statusText = Game.GetText(UIds.DIMENSION_MISSION_CLEAR_TICKET_ENDSCREEN_STATUS);
                rewardStatus = statusText.TryParseStatus();
            }

            Game.Click(UIds.DIMENSION_MISSION_CLEAR_TICKET_ENDSCREEN_CLOSE);
            await Task.Delay(2000);

            Game.Click(UIds.DIMENSION_MISSION_BACK_BUTTON);
            await Task.Delay(1000);

            Game.Click(UIds.DIMENSION_MISSION_REWARD_COLLECT_BUTTON);
            await Task.Delay(2000);

            if(!await WaitUntilVisible(UIds.DIMENSION_MISSION_REWARD_ACQUIRED_NOTICE))
            {
                Logger.LogError("Reward notice did not show up");
                return;
            }
            await Task.Delay(1000);
            Game.Click(UIds.DIMENSION_MISSION_REWARD_ACQUIRED_NOTICE_OK);
            await Task.Delay(1000);
        }
    }
}
