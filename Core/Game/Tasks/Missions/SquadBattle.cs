using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;
using autoplaysharp.Contracts.Errors;
using autoplaysharp.Game.Tasks;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace autoplaysharp.Core.Game.Tasks.Missions
{
    // TODO: press tab on end screen?
    public class SquadBattle : ContentStatusBoardDependenTask
    {
        public SquadBattle(IGame game, IUiRepository repository, ISettings settings) : base(game, repository, settings)
        {
        }

        protected override async Task RunCore(CancellationToken token)
        {
            var status = await StartContentBoardMission("SQUAD BATTLE");
            if(status.Completed)
            {
                Logger.LogInformation("Already completed...");
                return;
            }

            await Task.Delay(2000);

            if (Game.IsVisible(UIds.SQUAD_BATTLE_RANK_DROP))
            {
                Game.Click(UIds.SQUAD_BATTLE_RANK_DROP);
            }

            int leastPoints = int.MaxValue;
            int leastPointsX = 0;
            int leastPointsY = 1; // first row is 1 not 0.
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
                                Logger.LogDebug($"Found new least points. {points} {leastPointsX}/{leastPointsY}");
                            }
                        }
                    }
                    Logger.LogDebug(pointsStr);
                }
            }

            var chosenOpponent = Repository[$"SQUAD_BATTLE_SELECTION_{leastPointsY}_ROW", leastPointsX, 0];
            Game.Click(chosenOpponent);

            await Task.Delay(2000);

            await SelectHeroes();

            Game.Click(UIds.SQUAD_BATTLE_START_BATTLE);

            await Task.Delay(1000);

            if(Game.IsVisible(UIds.SQUAD_BATTLE_ALL_SLOTS_MUST_BE_FILLED))
            {
                Game.Click(UIds.SQUAD_BATTLE_ALL_SLOTS_MUST_BE_FILLED_OK);
                await Task.Delay(1000);
                await SelectHeroes();

                Game.Click(UIds.SQUAD_BATTLE_START_BATTLE);
            }

            Func<bool> endCondition = () => Game.IsVisible(UIds.SQUAD_BATTLE_END_BATTLE_MESSAGE);
            Func<bool> summaryBattlePoints = () => Game.IsVisible(UIds.SQUAD_BATTLE_SUMMARY_OVERALL_BATTLEPOINTS);
            var fightBot = new AutoFight(Game, Repository, Settings, endCondition, summaryBattlePoints);
            await fightBot.Run(token);

            await Task.Delay(2000);

            if(!await ClickWhenVisible(UIds.SQUAD_BATTLE_SUMMARY_OVERALL_BATTLEPOINTS, 10))
            {
                Logger.LogError("Rank up/down notification did not appear. Did we already run this mode?");
                return;
            }
            
            if (!await ClickWhenVisible(UIds.SQUAD_BATTLE_END_HOME_BUTTON))
            {
                Game.OnError(new ElementNotFoundError(Repository[UIds.SQUAD_BATTLE_END_HOME_BUTTON]));
                Logger.LogError("Rank up/down notification did not appear.");
                return;
            }

            await Task.Delay(2000);
        }

        private async Task SelectHeroes()
        {
            for (int i = 0; i < 3; i++)
            {
                Game.Click(Repository[UIds.SQUAD_BATTLE_HERO_SELECTION_DYN, i, 0]);
                await Task.Delay(250);
            }
        }
    }
}
