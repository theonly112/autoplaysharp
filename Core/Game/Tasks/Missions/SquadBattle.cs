using autoplaysharp.Contracts;
using autoplaysharp.Game.Tasks;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace autoplaysharp.Core.Game.Tasks.Missions
{
    public class SquadBattle : ContentStatusBoardDependenTask
    {
        public SquadBattle(IGame game, IUiRepository repository) : base(game, repository)
        {
        }

        protected override async Task RunCore(CancellationToken token)
        {
            var status = await StartContentBoardMission("SQUAD BATTLE");
            if(status.Completed)
            {
                Console.WriteLine("Already completed...");
                return;
            }

            await Task.Delay(2000);

            int leastPoints = int.MaxValue;
            int leastPointsX = 0;
            int leastPointsY = 0;
            for (int y = 1; y < 4; y++)
            {
                for (int x = 0; x < 2; x++)
                {
                    var pointsStr = Game.GetText(Repository[$"SQUAD_BATTLE_POINTS_{y}_ROW", x, 0]);
                    var trimStr = "Point(s)";
                    if (pointsStr.Contains(trimStr))
                    {
                        var pointSubString = pointsStr.Substring(0, pointsStr.Length - trimStr.Length).TrimEnd();
                        if(int.TryParse(pointSubString, NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var points))
                        {
                            if(points < leastPoints)
                            {
                                leastPointsX = x;
                                leastPointsY = y;
                                leastPoints = points;
                                Console.WriteLine($"Found new least points. {points} {leastPointsX}/{leastPointsY}");
                            }
                        }
                    }
                    Console.WriteLine(pointsStr);
                }
            }

            var chosenOpponent = Repository[$"SQUAD_BATTLE_SELECTION_{leastPointsY}_ROW", leastPointsX, 0];
            Game.Click(chosenOpponent);

            await Task.Delay(2000);

            for (int i = 0; i < 3; i++)
            {
                Game.Click(Repository["SQUAD_BATTLE_HERO_SELECTION_DYN", i, 0]);
                await Task.Delay(250);
            }

            Game.Click("SQUAD_BATTLE_START_BATTLE");

            Func<bool> endCondition = () => Game.IsVisible("SQUAD_BATTLE_END_BATTLE_MESSAGE");
            var fightBot = new AutoFight(Game, Repository, endCondition);
            await fightBot.Run(token);

            await Task.Delay(2000);

            Game.Click("SQUAD_BATTLE_END_HOME_BUTTON");
            await Task.Delay(5000);
        }
    }
}
