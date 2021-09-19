using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace autoplaysharp.Core.Game.Tasks.Missions
{
    public class WorldBossInvasion : GameTask
    {
        public WorldBossInvasion(IGame game, IUiRepository repository, ISettings settings) : base(game, repository, settings)
        {
        }

        private enum SelectionMode
        {
            Any,
            SelectFemales,
            SelectMales,
            SelectCombat,
            SelectSpeed,
            SelectBlast,
            SelectVillain
        }

        protected override async Task RunCore(CancellationToken token)
        {
            if (!await GoToMainScreen(token))
            {
                Logger.LogError("Could't go to main screen");
                return;
            }
            Game.Click(UIds.MAIN_MENU_ENTER);
            await Task.Delay(2000, token);
            Game.Click(UIds.WBI_SELECT_MISSION_COOP);
            await Task.Delay(2000, token);
            Game.Click(UIds.WBI_SELECT_MISSION_WBI);
            await Task.Delay(2000, token);
            Game.Click(UIds.WBI_MANAGE_SUPPLIES);
            await Task.Delay(2000, token);

            var emptySlots = await CollectChests();
            Logger.LogInformation($"{emptySlots} chest slots empty.");

            if (emptySlots == 0)
            {
                return;
            }

            SelectActiveOpponent();

            await Task.Delay(1000, token);

            // TODO: this info is unsed at the moment.
            var activeQuest = Game.GetText(UIds.WBI_COOP_ACTIVE_QUEST);
            Logger.LogDebug(activeQuest);

            SelectionMode selectionMode = SelectionMode.Any;
            bool useCoopSkill = false;
            switch (activeQuest)
            {
                case "Clear the stage with less than 3 Male Characters.":
                    selectionMode = SelectionMode.SelectFemales;
                    break;
                case "Clear the stage loosing 1 character or less.":
                case "Clear the stage while using Co-op Skills less than 5 times.":
                    selectionMode = SelectionMode.Any;
                    break;
                case "Clear the stage with more than 4 Combat Type Characters.":
                    selectionMode = SelectionMode.SelectCombat;
                    break;
                case "Clear the stage with less than 3 Combat Type Characters.":
                    selectionMode = SelectionMode.SelectSpeed; // Any is fine. just no combat.
                    break;
                case "Clear the stage with more than 4 Blast Type Characters":
                    selectionMode = SelectionMode.SelectBlast;
                    break;
                case "Clear the stage with less than 5 Super Heroes.":
                    selectionMode = SelectionMode.SelectVillain;
                    break;
                case "Clear the stage while using Co-op Skills more than 1 times.":
                    useCoopSkill = true;
                    Logger.LogDebug("Will try to use coop skill for WBI mission.");
                    break;
            }

            await WaitUntilVisible(UIds.WBI_OPPONENT_ENTER);
            await Task.Delay(250, token);
            Game.Click(UIds.WBI_OPPONENT_ENTER);
            await Task.Delay(2000, token);

            await CollectNewChests(emptySlots, selectionMode, token, useCoopSkill);

            await GoToMainScreen(token);
        }

        private async Task CollectNewChests(int emptySlots, SelectionMode selectionMode, CancellationToken token,
            bool useCoopSkill)
        {
            for (var i = 0; i < emptySlots; i++)
            {
                if (!await WaitUntilVisible(UIds.WBI_HERO_START_MISSION, token))
                {
                    Logger.LogError("Start button did not appear in time.");
                    return;
                }

                await SelectCharacters(selectionMode);
                await Task.Delay(2000, token);
                Logger.LogDebug("Starting mission");
                Game.Click(UIds.WBI_HERO_START_MISSION);

                await Fight(token, useCoopSkill);

                if(await HandleHeroicQuestNotice())
                {
                    // We are back to the main screen. so we have to restart.
                    // TODO: is this the best way to do this?
                    await Run(token);
                    break;
                }
            }
        }

        private async Task Fight(CancellationToken token, bool useCoopSkill)
        {
            Func<bool> chestDropped = () => { return Game.IsVisible(UIds.WBI_SUPPLY_CHEST); };
            Func<bool> disconnected = () => { return Game.IsVisible(UIds.GENERIC_MISSION_NOTICE_DISCONNECTED); };

            var fightBot = new AutoFight(Game, Repository, Settings, 90, chestDropped, disconnected);
            fightBot.UseCoopSkill = useCoopSkill;
            await fightBot.Run(token);

            if (Game.IsVisible(UIds.GENERIC_MISSION_NOTICE_DISCONNECTED))
            {
                Game.Click(UIds.GENERIC_MISSION_NOTICE_DISCONNECTED_OK);
                await Task.Delay(4000, token);

                Logger.LogInformation("Restarting because of disconnect");

                Game.Click(UIds.WBI_HERO_START_MISSION);

                await Fight(token, useCoopSkill);
            }
            else
            {
                await Task.Delay(2000, token);

                Game.Click(UIds.WBI_NEXT_RUN);
            }
        }

        private async Task SelectCharacters(SelectionMode selectionMode)
        {
            UiElement FindTypeRow(string type)
            {
                for (int i = 0; i < 10; i++)
                {
                    var item = Repository[UIds.WBI_HERO_SELECTION_TYPE_CB_DYN, 0, i];
                    var text = Game.GetText(item);
                    if(text == type)
                    {
                        return item;
                    }
                }
                throw new ArgumentException(nameof(type), $"Did not find {type} character type.");
            }

            Game.Click(UIds.WBI_HERO_SELECTION_TYPE_CB);
            await Task.Delay(500);
            switch (selectionMode)
            {
                case SelectionMode.Any:
                    Game.Click(Repository[UIds.WBI_HERO_SELECTION_TYPE_CB_DYN, 0, 0]);
                    break;
                case SelectionMode.SelectFemales:
                    Game.Click(FindTypeRow("FEMALE"));
                    break;
                case SelectionMode.SelectMales:
                    Game.Click(FindTypeRow("MALE"));
                    break;
                case SelectionMode.SelectCombat:
                    Game.Click(FindTypeRow("COMBAT"));
                    break;
                case SelectionMode.SelectSpeed:
                    Game.Click(FindTypeRow("SPEED"));
                    break;
                case SelectionMode.SelectBlast:
                    Game.Click(FindTypeRow("BLAST"));
                    break;
                case SelectionMode.SelectVillain:
                    Game.Click(FindTypeRow("SUPER VILLAIN"));
                    break;
            }

            await Task.Delay(500);

            Logger.LogDebug("Selecting characters");
            for (int i = 0; i < 3; i++)
            {
                var hero = Repository[UIds.WBI_HERO_SELECTION_DYN, i, 0];
                Game.Click(hero);
                await Task.Delay(250);
            }
        }

        private void SelectActiveOpponent()
        {
            var type = Game.GetText(Repository[UIds.WBI_OPPONENT_TYPE]);

            string opponentFmtStr;
            switch (type)
            {
                case "Twilight":
                     opponentFmtStr = "WBI_TWILIGHT_OPPONENT_{0}_ACTIVE";
                    break;
                case "Thanos and the Black Order":
                    opponentFmtStr = "WBI_BLACKORDER_OPPONENT_{0}_ACTIVE";
                    break;
                default:
                    Logger.LogError($"Unknown WBI Opponent type: {type}");
                    throw new Exception("Unhandled");
            }

            for (int i = 1; i < 8; i++)
            {
                var id = string.Format(opponentFmtStr, i);
                var opponent = Repository[id];
                var time = Game.GetText(opponent);
                if (time.Any(char.IsNumber) && (time.Contains("h") || time.Contains("m"))) // TODO: can we make this more robust?
                {
                    Game.Click(opponent);
                }
            }
        }

        private async Task<int> CollectChests()
        {
            int availableChests = 0;
            for (int i = 0; i < 5; i++)
            {
                var chestAvailable = Repository[UIds.WBI_MANAGE_SUPPLY_CHESTS_ACQUIRE, i, 0];
                if (Game.IsVisible(chestAvailable))
                {
                    Logger.LogInformation($"Chest {i + 1} available for pick up");
                    Game.Click(chestAvailable);
                    await Task.Delay(2000);
                    Game.Click(UIds.WBI_MANAGE_SUPPLIES_SKIP);
                    await Task.Delay(2000);
                    Game.Click(UIds.WBI_MANAGE_SUPPLIES_SKIP);
                    await Task.Delay(2000);

                    availableChests++;
                    continue;
                }

                var chestEmpty = Repository[UIds.WBI_MANAGE_SUPPLY_CHESTS_EMPTY_SLOT, i, 0];
                if (Game.IsVisible(chestEmpty))
                {
                    availableChests++;
                }
            }

            await Task.Delay(1000);
            Game.Click(UIds.WBI_MANAGE_SUPPLIES_SELECT_STAGE);
            await Task.Delay(2000);

            return availableChests;
        }
    }
}
