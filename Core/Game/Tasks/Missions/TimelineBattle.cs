using System.Threading;
using System.Threading.Tasks;
using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;
using Microsoft.Extensions.Logging;

namespace autoplaysharp.Core.Game.Tasks.Missions
{
    /// <summary>
    /// TODOs:
    /// 1. disable auto repeat button if enabled.
    /// 2. make it more reliable. should be prio 1. lol
    /// 3. use autofight instead of ingame "Autoplay+".
    /// 4. handle end notice.
    /// </summary>
    public class TimelineBattle : ContentStatusBoardDependenTask
    {
        private const string _missionName = "TIMELINE BATTLE";
        public TimelineBattle(IGame game, IUiRepository repository, ISettings settings) : base(game, repository, settings)
        {
            Team = settings.TimelineBattle.Team;
        }

        public int Team { get; set; } = 1;

        protected override async Task RunCore(CancellationToken token)
        {
            while(true)
            {
                var status = await StartContentBoardMission(_missionName);
                if (status == null)
                {
                    Logger.LogError($"Failed to start {_missionName}");
                    return;
                }

                Logger.LogInformation($"Timeline Battle: {status.Available} runs available.");

                if(status.Available <  1)
                {
                    Logger.LogInformation("Timeline Battle done...");
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

                var team = Repository[UIds.TIMELINE_TEAM_SELECTION_DYN, 0, Team - 1];
                Game.Click(team);

                await Task.Delay(1000);

                if (Game.IsVisible(UIds.TIMELINE_AUTO_REPEAT))
                {
                    Logger.LogDebug("Disabling auto-repeat");
                    Game.Click(UIds.TIMELINE_AUTO_REPEAT);
                    await Task.Delay(250);
                }

                Game.Click("TIMELINE_SEARCH_FOR_OPPONENT");

                if (!await WaitUntilVisible("TIMELINE_FIGHT"))
                {
                    return;
                }

                await Task.Delay(2000);
                Game.Click("TIMELINE_FIGHT");


                Logger.LogInformation("Waiting for fight to finish");
                if (!await WaitUntilVisible("TIMELINE_ENDSCREEN_HOME_BUTTON_IMAGE", token, 120, 5)) 
                {
                    Logger.LogError("Home button did not appear.");
                    return;
                }

                Game.Click("TIMELINE_ENDSCREEN_HOME_BUTTON_IMAGE");

                await Task.Delay(1000);

                await HandleEndNotices();
                await Task.Delay(1000);
            }

        }
    }
}
