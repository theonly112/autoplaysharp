using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Errors;
using autoplaysharp.Core.Helper;
using autoplaysharp.Game.Tasks;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace autoplaysharp.Core.Game.Tasks.Missions
{
    public class LegendaryBattle : ContentStatusBoardDependenTask
    {
        public LegendaryBattle(IGame game, IUiRepository repository) : base(game, repository)
        {
        }

        public string BattleName { get; set; } = "MARVEL'S AVENGERS: ENDGAME";
        public int ClearCount { get; set; } = 5;

        protected override async Task RunCore(CancellationToken token)
        {
            var status = await StartContentBoardMission("LEGENDARY BATTLE");
            if(status == null)
            {
                Logger.LogError("Could not find legendary battle");
                return;
            }

            if (status.Completed)
            {
                Logger.LogInformation("Already completed");
                return;
            }

            if(ClearCount > status.Available)
            {
                Logger.LogError($"Cannot run {ClearCount} times. Only {status.Available} available");
                return;
            }
            else
            {
                Logger.LogDebug($"Trying to clear legendary battle {ClearCount} times.");
            }

            await Task.Delay(1000);

            bool found = false;
            // TODO: implement search for mission via drag
            for (int y = 0; y < 4; y++)
            {
                var mission = Repository["LEGENDARY_BATTLE_NAME_DYN", 0, y];
                var missionText = Game.GetText(mission);
                if (missionText.Similarity(BattleName) > 0.9) // Accept small changes. TODO: figure out if this is sufficent.
                {
                    found = true;
                    Game.Click(mission);
                    break;
                }
            }

            if(!found)
            {
                Logger.LogError($"Failed to find mission {BattleName}.");
                Game.OnError(new ElementNotFoundError(Repository["LEGENDARY_BATTLE_NAME_DYN"]));
            }

            await Task.Delay(1000);

            Game.Click("LEGENDARY_BATTLE_NORMAL_MODE_BUTTON");
            Logger.LogDebug("Starting normal mode");

            await Task.Delay(1000);

            var enter = Repository["LEGENDARY_BATTLE_MISSION_ENTER", 0, 0];

            if (!await WaitUntilVisible(enter))
            {
                Logger.LogError("Enter button did not appear");
                Game.OnError(new ElementNotFoundError(enter));
                return;
            }

            Game.Click(enter);
            Logger.LogDebug("Entering mission");


            for (int i = 0; i < ClearCount; i++)
            {
                Logger.LogDebug($"Running mission (1/{status.Available})");

                await Task.Delay(3000);

                Logger.LogDebug("Starting mission.");
                Game.Click("LEGENDARY_BATTLE_MISSION_START");
                
                
                Logger.LogDebug("Waiting for skip into button");
                
                if (await WaitUntilVisible("LEGENDARY_BATTLE_MISSION_SKIP_INTRO", 10))
                {
                    Game.Click("LEGENDARY_BATTLE_MISSION_SKIP_INTRO");
                }

                Logger.LogDebug("Starting auto fight");

                Func<bool> end = () => Game.IsVisible("LEGENDARY_BATTLE_MISSION_SUCCESS");
                var fightBot = new AutoFight(Game, Repository, end);
                await fightBot.Run(token);

                await Task.Delay(1000);
                
                Logger.LogDebug("Starting auto fight finished. Repeating run.");
                Game.Click("LEGENDARY_BATTLE_ENDSCREEN_REPEAT_RUN");

            }

            await Task.Delay(2000);
            // Handle heroic quest notices.
            await GoToMainScreen();
        }
    }
}
