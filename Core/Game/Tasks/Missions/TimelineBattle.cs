using autoplaysharp.Contracts;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace autoplaysharp.Game.Tasks.Missions
{
    /// <summary>
    /// TODOs:
    /// 1. disable repeat button if enabled.
    /// 2. make it more reliable. should be prio 1. lol
    /// 3. use autofight instead of ingame "Autoplay+".
    /// 4. fix issue with available run count.
    /// </summary>
    public class TimelineBattle : ContentStatusBoardDependenTask
    {
        private const string _missionName = "TIMELINE BATTLE";
        public TimelineBattle(IGame game, IUiRepository repository) : base(game, repository)
        {
        }

        protected override async Task RunCore(CancellationToken token)
        {
            while(true)
            {
                var status = await StartContentBoardMission(_missionName);
                if (status == null)
                {
                    Console.WriteLine($"Failed to start {_missionName}");
                    return;
                }

                Console.WriteLine($"Timeline Battle: {status.Available} runs available.");

                if(status.Available <  1)
                {
                    Console.WriteLine("Timeline Battle done...");
                    return;
                }


                if (!await WaitUntilVisible("TIMELINE_GET_READY"))
                {
                    return;
                }

                await Task.Delay(1000);
                Game.Click("TIMELINE_GET_READY");

                if (!await WaitUntilVisible("TIMELINE_SEARCH_FOR_OPPONENT"))
                {
                    return;
                }

                await Task.Delay(1000);
                Game.Click("TIMELINE_SEARCH_FOR_OPPONENT");

                if (!await WaitUntilVisible("TIMELINE_FIGHT"))
                {
                    return;
                }

                await Task.Delay(2000);
                Game.Click("TIMELINE_FIGHT");


                Console.WriteLine("Waiting for fight to finish");
                if (!await WaitUntilVisible("TIMELINE_ENDSCREEN_HOME_BUTTON_IMAGE", token, 120, 5)) 
                {

                }

                Game.Click("TIMELINE_ENDSCREEN_HOME_BUTTON_IMAGE");

                await WaitUntil(IsOnMainScreen, token);
                await Task.Delay(1000);
            }

        }
    }
}
