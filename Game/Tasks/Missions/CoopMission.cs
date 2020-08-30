using autoplaysharp.Contracts;
using System;
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
    class CoopMission : ContentStatusBoardDependenTask
    {
        public CoopMission(IGame game, IUiRepository repository) : base(game, repository)
        {
        }
         
        protected override async Task RunCore(CancellationToken token)
        {
            if(!await StartContentBoardMission("CO-OP PLAY"))
            {
                return;
            }

            await Task.Delay(2000);

            while(true)
            {
                var text = Game.GetText("COOP_REWARD_COUNT");
                var match = ContentStatus.StatusRegex.Match(text);
                if (!match.Success)
                {
                    Console.WriteLine("Could not get avilable reward count");
                    return;
                }

                if (!int.TryParse(match.Groups[1].Value, out var num))
                {
                    Console.WriteLine("Could not get avilable reward count. Try parse failed.");
                    return;
                }

                if (num == 0)
                {
                    Console.WriteLine("Done with co-op. No rewards available.");
                    return;
                }

                if (Game.IsVisible("COOP_REWARD_ACQUIRED"))
                {
                    Game.Click("COOP_REWARD_ACQUIRED");
                    await Task.Delay(5000);
                    Game.Click("COOP_REWARD_ACQUIRE_REWARD");
                    await Task.Delay(5000);
                    Game.Click("COOP_REWARD_ACQUIRE_REWARD_OK");
                    await Task.Delay(5000);
                }

                if (!Game.IsVisible("COOP_START"))
                {
                    Game.Click("COOP_DEPLOY_CHARACTER");
                }

                if (!await WaitUntilVisible("COOP_START", token))
                {
                    Console.WriteLine("Start button not available.");
                    return;
                }
                Game.Click("COOP_START");

                if (!await WaitUntilVisible("COOP_ENDSCREEN_MISSION_SUCCESS", token, 60, 1))
                {
                    Console.WriteLine("Wait on success message failed.");
                    return;
                }

                await Task.Delay(5000);

                Game.Click("COOP_ENDSCREEN_NEXT_BUTTON");

                await Task.Delay(5000);
            }

        }
    }
}
