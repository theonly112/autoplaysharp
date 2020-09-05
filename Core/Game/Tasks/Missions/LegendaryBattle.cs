using autoplaysharp.Contracts;
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

        protected override async Task RunCore(CancellationToken token)
        {
            var status = await StartContentBoardMission("LEGENDARY BATTLE");
            if (status.Completed)
            {
                Logger.LogInformation("Already completed");
                return;
            }

            var missionToRun = "MARVEL'S AVENGERS: ENDGAME";

            await Task.Delay(1000);

            // TODO: implement search for mission via drag
            for (int y = 0; y < 4; y++)
            {
                var mission = Repository["LEGENDARY_BATTLE_NAME_DYN", 0, y];
                var missionText = Game.GetText(mission);
                if (missionText == missionToRun)
                {
                    Game.Click(mission);
                    break;
                }
            }

            await Task.Delay(1000);

            Game.Click("LEGENDARY_BATTLE_NORMAL_MODE_BUTTON");

            await Task.Delay(1000);

            var enter = Repository["LEGENDARY_BATTLE_MISSION_ENTER", 0, 0];

            if (!await WaitUntilVisible(enter))
            {
                Logger.LogError("Enter button did not appear");
                return;
            }

            Game.Click(enter);

            for (int i = 0; i < status.Available; i++)
            {
                await Task.Delay(3000);

                Game.Click("LEGENDARY_BATTLE_MISSION_START");

                if (await WaitUntilVisible("LEGENDARY_BATTLE_MISSION_SKIP_INTRO", 10))
                {
                    Game.Click("LEGENDARY_BATTLE_MISSION_SKIP_INTRO");
                }

                Func<bool> end = () => Game.IsVisible("LEGENDARY_BATTLE_MISSION_SUCCESS");
                var fightBot = new AutoFight(Game, Repository, end);
                await fightBot.Run(token);

                await Task.Delay(1000);

                Game.Click("LEGENDARY_BATTLE_ENDSCREEN_REPEAT_RUN");
            }
        }
    }
}
