using autoplaysharp.Contracts;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace autoplaysharp.Game.Tasks.Missions
{
    public class AllianceBattle : ContentStatusBoardDependenTask
    {
        public AllianceBattle(IGame game, IUiRepository repository) : base(game, repository)
        {
        }

        protected override async Task RunCore(CancellationToken token)
        {
            const string MissionName = "ALLIANCE BATTLE";
            if(await StartContentBoardMission(MissionName) == null)
            {
                Console.WriteLine($"Failed to go to {MissionName}");
                return;
            }
            await WaitUntilVisible("ALLIANCE_BATTLE_MODE_HEADER");


            await RunNormalMode(token);

            if (await StartContentBoardMission(MissionName) == null)
            {
                Console.WriteLine($"Failed to go to {MissionName}");
                return;
            }
            await WaitUntilVisible("ALLIANCE_BATTLE_MODE_HEADER");

            await RunExtremeMode(token);

        }

        private async Task RunExtremeMode(CancellationToken token)
        {
            if (!await WaitUntilVisible("ALLIANCE_BATTLE_EXTREME_MODE_READY"))
            {
                Console.WriteLine("Extreme mode not available.");
                return;
            }
            Game.Click("ALLIANCE_BATTLE_EXTREME_MODE_READY");

            await SelectHeroes();

            Game.Click("ALLIANCE_BATTLE_EXTREME_MODE_START");

            if (!await HandleStartNotices())
            {
                Console.WriteLine("Failed to start mission...");
                return;
            }

            if(!await RunAutoFight(token))
            {
                Console.WriteLine("Failed to run autofight");
                return;
            }
        }

        private async Task RunNormalMode(CancellationToken token)
        {
            if (!await WaitUntilVisible("ALLIANCE_BATTLE_NORMAL_MODE_READY", token))
            {
                Console.WriteLine("Normal mode not available.");
                return;
            }

            Game.Click("ALLIANCE_BATTLE_NORMAL_MODE_READY");


            if (await WaitUntilVisible("ALLIANCE_BATTLE_NORMAL_MODE_START", token))
            {
                Console.WriteLine("Normal mode start button not available.");
                return;
            }

            await SelectHeroes();

            Game.Click("ALLIANCE_BATTLE_NORMAL_MODE_START");

            await Task.Delay(500);

            if (!await HandleStartNotices())
            {
                Console.WriteLine("Failed to start mission...");
                return;
            }

            var autoFight = new AutoFight(Game, Repository);
            await autoFight.Run(token);

            await Task.Delay(1000);

            Game.Click("ALLIANCE_BATTLE_END_SCREEN_HOME");

            await Task.Delay(5000);
        }

        private async Task SelectHeroes()
        {
            if (!await WaitUntilVisible("ALLIANCE_BATTLE_HERO_SELECTION_HEADER"))
            {
                Console.WriteLine("Failed: Hero selection screen did not appear");
                return;
            }

            for (int i = 0; i < 3; i++)
            {
                Game.Click(Repository["ALLIANCE_BATTLE_HERO_SELECTION_DYN", i, 0]);
                await Task.Delay(200);
            }
        }

        private async Task<bool> RunAutoFight(CancellationToken token)
        {
            var autoFight = new AutoFight(Game, Repository, () => Game.IsVisible("ALLIANCE_BATTLE_ENDED_MESSAGE"), () => Game.IsVisible("ALLIANCE_BATTLE_CLEAR_MESSAGE"));
            var autoFightTask = autoFight.Run(token);

            if(await Task.WhenAny(autoFightTask, Task.Delay(300*1000)) == autoFightTask)
            {
                await autoFightTask; // catch exception if thrown...
                return true;
            }

            return false;
        }
    }
}
