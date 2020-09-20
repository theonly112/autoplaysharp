using autoplaysharp.Contracts;
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
        public CoopMission(IGame game, IUiRepository repository) : base(game, repository)
        {
        }
         
        protected override async Task RunCore(CancellationToken token)
        {
            if(await StartContentBoardMission("CO-OP PLAY") == null)
            {
                return;
            }

            await Task.Delay(2000);

            while(true)
            {
                var text = Game.GetText(UIds.COOP_REWARD_COUNT);
                var match = ContentStatus.StatusRegex.Match(text);
                if (!match.Success)
                {
                    Logger.LogError("Could not get avilable reward count");
                    return;
                }

                if (!int.TryParse(match.Groups[1].Value, out var num))
                {
                    Logger.LogError("Could not get avilable reward count. Try parse failed.");
                    return;
                }

                if (num == 0)
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
                }

                if(Game.IsVisible(UIds.COOP_AUTO_REPEAT_IMAGE))
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

                if(!await HandleStartNotices())
                {
                    Logger.LogError("Unable to handle mission start notice.");
                    return;
                }

                if (!await WaitUntilVisible(UIds.COOP_ENDSCREEN_MISSION_SUCCESS, token, 60, 1))
                {
                    if(Game.IsVisible(UIds.COOP_REWARD_NOTICE_DAILY_LIMIT))
                    {
                        Game.Click(UIds.COOP_REWARD_NOTICE_DAILY_LIMIT_OK);
                        await Task.Delay(2000);
                        Logger.LogInformation("Finished CO-OP missions.");
                        return;
                    }
                    Logger.LogError("Wait on success message failed.");
                    return;
                }

                await Task.Delay(5000);

                Game.Click(UIds.COOP_ENDSCREEN_NEXT_BUTTON);

                await Task.Delay(5000);
            }

        }
    }
}
