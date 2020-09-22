using autoplaysharp.Contracts;
using autoplaysharp.Game.Tasks;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace autoplaysharp.Core.Game.Tasks.Missions
{
    public class WorldBossInvasion : GameTask
    {
        public WorldBossInvasion(IGame game, IUiRepository repository) : base(game, repository)
        {
        }

        protected override async Task RunCore(CancellationToken token)
        {
            if (!await GoToMainScreen())
            {
                Logger.LogError("Could't go to main screen");
                return;
            }
            Game.Click(UIds.MAIN_MENU_ENTER);
            await Task.Delay(2000);
            Game.Click(UIds.WBI_SELECT_MISSION_COOP);
            await Task.Delay(2000);
            Game.Click(UIds.WBI_SELECT_MISSION_WBI);
            await Task.Delay(2000);
            Game.Click(UIds.WBI_MANAGE_SUPPLIES);
            await Task.Delay(2000);

            var emptySlots = await CollectChests();
            Logger.LogInformation($"{emptySlots} chest slots empty.");

            if (emptySlots == 0)
            {
                return;
            }

            SelectActiveOpponent();
            await WaitUntilVisible(UIds.WBI_OPPONENT_ENTER);
            await Task.Delay(250);
            Game.Click(UIds.WBI_OPPONENT_ENTER);
            await Task.Delay(2000);

            await CollectNewChests(emptySlots, token);

            await GoToMainScreen();
        }

        private async Task CollectNewChests(int emptySlots, CancellationToken token)
        {
            for (int i = 0; i < emptySlots; i++)
            {
                if (!await WaitUntilVisible(UIds.WBI_HERO_START_MISSION))
                {
                    Logger.LogError("Start button did not appear in time.");
                    return;
                }

                await SelectCharacters();
                await Task.Delay(2000);
                Game.Click(UIds.WBI_HERO_START_MISSION);

                await Fight(token);

                await HandleHeroicQuestNotice();
            }
        }

        private async Task Fight(CancellationToken token)
        {
            Func<bool> slotChest = () => { return Game.IsVisible(UIds.WBI_SLOT_CHEST_IN_INVENTORY); };
            Func<bool> disconnected = () => { return Game.IsVisible(UIds.GENERIC_MISSION_NOTICE_DISCONNECTED); };
            var fightBot = new AutoFight(Game, Repository, 90, slotChest, disconnected);
            await fightBot.Run(token);

            if (Game.IsVisible(UIds.GENERIC_MISSION_NOTICE_DISCONNECTED))
            {
                Game.Click(UIds.GENERIC_MISSION_NOTICE_DISCONNECTED_OK);
                await Task.Delay(2000);

                Logger.LogInformation("Restarting because of disconnect");

                Game.Click(UIds.WBI_HERO_START_MISSION);

                await Fight(token);
                return;
            }
            else
            {
                await Task.Delay(2000);

                Game.Click(UIds.WBI_SLOT_CHEST_IN_INVENTORY);

                await Task.Delay(2000);

                Game.Click(UIds.WBI_NEXT_RUN);
            }
        }

        private async Task SelectCharacters()
        {
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
                if (time.Contains("h") || time.Contains("m")) // TODO: can we make this more robust?
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
                    continue;
                }
            }

            await Task.Delay(1000);
            Game.Click(UIds.WBI_MANAGE_SUPPLIES_SELECT_STAGE);
            await Task.Delay(2000);

            return availableChests;
        }
    }
}
