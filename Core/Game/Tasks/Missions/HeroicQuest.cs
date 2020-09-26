using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;
using autoplaysharp.Core.Game.Tasks.Inventory;
using autoplaysharp.Core.Helper;
using autoplaysharp.Game.Tasks;
using autoplaysharp.Game.Tasks.Missions;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace autoplaysharp.Core.Game.Tasks.Missions
{
    public class HeroicQuest : GameTask
    {
        public HeroicQuest(IGame game, IUiRepository repository, ISettings settings) : base(game, repository, settings)
        {
        }

        protected override async Task RunCore(CancellationToken token)
        {
            if(!await GoToMainScreen(token))
            {
                Logger.LogError("Failed to go to main screen");
                return;
            }

            Game.Click(UIds.MAIN_MENU_ENTER);
            
            if(!await WaitUntilVisible(UIds.HEROIC_QUEST_HEADER))
            {
                Logger.LogError("Could not find heroic quest");
                return;
            }

            Game.Click(UIds.HEROIC_QUEST_HEADER);

            await Task.Delay(1000, token);

            if(Game.IsVisible(UIds.HEROIC_QUEST_CYSTAL_CHEST_FULL_NOTICE))
            {
                Logger.LogDebug("Clsing crytsal chest full notice.");
                Game.Click(UIds.HEROIC_QUEST_CYSTAL_CHEST_FULL_NOTICE_OK);
                await Task.Delay(1000, token);
            }
            
            if(Game.IsVisible(UIds.HEROIC_QUEST_CYSTAL_CHEST_NOTICE))
            {
                Logger.LogDebug("Closing crystal chest notice.");
                Game.Click(UIds.HEROIC_QUEST_CYSTAL_CHEST_NOTICE_CANCEL);
                await Task.Delay(1000, token);
            }

            var id = UIds.HEROIC_QUEST_JESSICA_NEW_YORK;
            var text = Game.GetText(id);
            if(text.Contains("Time Remaining"))
            {
                Logger.LogDebug($"Detecting running mission {id}");
                Game.Click(id);
                await Task.Delay(1000, token);
            }

            if(Game.IsVisible(UIds.HEROIC_QUEST_SKIP_INTRO))
            {
                Logger.LogDebug("Skipping intro.");
                Game.Click(UIds.HEROIC_QUEST_SKIP_INTRO);
                await Task.Delay(1000, token);
            }

            // TODO: not sure if this it the correct.
            // Should this also be HEROIC_QUEST_TAB_THE_SCREEN_TO_CONTINUE?
            if (Game.IsVisible(UIds.HEROIC_QUEST_TAB_THE_SCREEN))
            {
                Logger.LogDebug("Taping screen to continue.");
                Game.Click(UIds.HEROIC_QUEST_TAB_THE_SCREEN);
                await Task.Delay(1000, token);
            }

            if (Game.IsVisible(UIds.HEROIC_QUEST_QUEST_INFO_ACQUIRE))
            {
                Logger.LogDebug("Attempting to acquire heroic quest reward.");
                Game.Click(UIds.HEROIC_QUEST_QUEST_INFO_ACQUIRE);
                await Task.Delay(1000, token);
                await TabToContinue(token);
            }

            await Task.Delay(1000);
            var questInfo = Game.GetText(UIds.HEROIC_QUEST_QUEST_INFO);
            var completionStatusText = Game.GetText(UIds.HEROIC_QUEST_QUEST_INFO_STATUS);
            var completionStatus = completionStatusText.TryParseStatus();

            Logger.LogDebug($"Current quest: {questInfo}");
            Logger.LogDebug($"Current quest status: {completionStatus}");

            switch (questInfo)
            {
                case var s when questInfo.StartsWith("[DIMENSION MISSION]"):
                    await HandleDimensionMissionQuest(questInfo, completionStatus, token);
                    break;
                case var s when new Regex(@"Use .* Gold").IsMatch(questInfo):
                    await HandleUseGoldMission(questInfo, completionStatus, token);
                    break;
                case var s when questInfo.StartsWith("[ALLIANCE BATTLE]"):
                    await HandleAllianceBattleQuest(questInfo, completionStatus, token);
                    break;
                case var s when questInfo.StartsWith("[CUSTOM GEAR]"):
                    await HandleCustomGearQuest(questInfo, completionStatus, token);
                    break;
                case var s when questInfo.StartsWith("[ENCHANTED URU]"):
                    await HandleUruQuest(questInfo, completionStatus, token);
                    break;
                case var s when questInfo.StartsWith("Beat World Boss"):
                    await HandleWorldBossQuest(questInfo, completionStatus, token);
                    break;
                case var s when questInfo.StartsWith("[ISO-8]"):
                    await HandleIso8Quest(questInfo, completionStatus, token);
                    break;
                case var s when questInfo.StartsWith("[DANGER ROOM"):
                    await HandleDangerRoomQuest(questInfo, completionStatus, token);
                    break;
                case var s when new Regex(@"Use .* Energy").IsMatch(questInfo):
                    await HandleUseEnergQuest(questInfo, completionStatus, token);
                    break;
                case var s when questInfo.StartsWith("[LEGENDARY BATTLE]"):
                    await HandleLegendaryBattleQuest(questInfo, completionStatus, token);
                    break;
                case var s when questInfo.StartsWith("[TIMELINE BATTLE"):
                    await HandleTimelineBattle(questInfo, completionStatus, token);
                    break;
                case var s when questInfo.StartsWith("[WORLD BOSS INVASION]"):
                    await HandleWorldBossInvasionQuest(questInfo, completionStatus, token);
                    break;
                case var s when questInfo.StartsWith("[CARD]"):
                    await HandleComicCardQuest(questInfo, completionStatus, token);
                    break;
                case var s when questInfo.StartsWith("[CO-OP PLAY]"):
                    await HandleCoopQuest(questInfo, completionStatus, token);
                    break;
                default:
                    Logger.LogError($"Unhandled heroic quest: {questInfo}");
                    break;
            }

            // TODO: right here we could loop.
        }

        private async Task HandleCoopQuest(string questInfo, (bool Success, int Current, int Max) completionStatus, CancellationToken token)
        {
            if(questInfo.Contains("Acquire"))
            {
                var coop = new CoopMission(Game, Repository, Settings);
                coop.RewardCount = 2; //TODO: are there other quests like this.
                await coop.Run(token);
            }
        }

        private async Task HandleComicCardQuest(string questInfo, (bool Success, int Current, int Max) completionStatus, CancellationToken token)
        {
            if(questInfo.Contains("Upgrade"))
            {
                var upgrade = new UpgradeCards(Game, Repository, Settings);
                await upgrade.Run(token);
            }
        }

        private async Task HandleWorldBossInvasionQuest(string questInfo, (bool Success, int Current, int Max) completionStatus, CancellationToken token)
        {
            Logger.LogError($"This quest type is not handled yet: {questInfo}");

            if (questInfo.Contains("Complete"))
            {
                // TODO: handle WBI COOP missions...
                var wbi = new WorldBossInvasion(Game, Repository, Settings);
                await wbi.Run(token);
            }
        }

        private async Task HandleTimelineBattle(string questInfo, (bool Success, int Current, int Max) completionStatus, CancellationToken token)
        {
            var timelineBattle = new TimelineBattle(Game, Repository, Settings);
            // TODO: settings?!?!?!
            await timelineBattle.Run(token);
        }

        private async Task HandleLegendaryBattleQuest(string questInfo, (bool Success, int Current, int Max) completionStatus, CancellationToken token)
        {
            Logger.LogDebug("Running legendary battle to complete heroic quest.");
            var legendaryBattle = new LegendaryBattle(Game, Repository, Settings);
            legendaryBattle.ClearCount = completionStatus.Max - completionStatus.Current;
            await legendaryBattle.Run(token);
        }

        private Task HandleUseEnergQuest(string questInfo, (bool Success, int Current, int Max) completionStatus, CancellationToken token)
        {
            // TODO: run some mission to spend energy...
            return Task.CompletedTask;
        }

        private async Task HandleDangerRoomQuest(string questInfo, (bool Success, int Current, int Max) completionStatus, CancellationToken token)
        {
            if(questInfo.Contains("Participate"))
            {
                // Text like: "[DANGER ROOM] Participate 1 time
                var dangerRoom = new DangerRoom(Game, Repository, Settings);
                dangerRoom.SingleRun = true;
                await dangerRoom.Run(token);
            }
        }

        private async Task HandleIso8Quest(string questInfo, (bool Success, int Current, int Max) completionStatus, CancellationToken token)
        {
            if(questInfo.Contains("Enhance"))
            {
                var enhance = new EnhanceIso8(Game, Repository, Settings);
                await enhance.Run(token);
            }
            else if(questInfo.Contains("Combine"))
            {
                var combine = new CombineIso8(Game, Repository, Settings);
                await combine.Run(token);
            }
        }

        private Task HandleWorldBossQuest(string questInfo, (bool Success, int Current, int Max) completionStatus, CancellationToken token)
        {
            if(questInfo.Contains("Participate"))
            {
                // TODO: dont really run world boss. just exit immediatly.
            }

            // TODO: run world boss...
            return Task.CompletedTask;
        }

        private async Task HandleUruQuest(string questInfo, (bool Success, int Current, int Max) completionStatus, CancellationToken token)
        {
            if(questInfo.Similarity("[ENCHANTED URU] Combine 3 times") > 0.8)
            {
                var upgradeUru = new CombineUru(Game, Repository, Settings);
                await upgradeUru.Run(token);
            }
        }

        private async Task HandleCustomGearQuest(string questInfo, (bool Success, int Current, int Max) completionStatus, CancellationToken token)
        {
            if (questInfo.Similarity("[CUSTOM GEAR] Upgrade x2") > 0.80)
            {
                var upgradeCustomGear = new UpgradeCustomGear(Game, Repository, Settings);
                await upgradeCustomGear.Run(token);
            }
        }

        private async Task HandleAllianceBattleQuest(string questInfo, (bool Success, int Current, int Max) completionStatus, CancellationToken token)
        {
            if(questInfo.Similarity("[ALLIANCE BATTLE] Participate in Normal Mode") > 0.80)
            {
                var allianceBattle = new AllianceBattle(Game, Repository, Settings);
                await allianceBattle.RunNormalMode(token);
            }
            else if(questInfo.Contains("Points in Normal Mode"))
            {
                var allianceBattle = new AllianceBattle(Game, Repository, Settings);
                await allianceBattle.RunNormalMode(token);
            }
        }

        private Task HandleUseGoldMission(string questInfo, (bool Success, int Current, int Max) completionStatus, CancellationToken token)
        {
            Logger.LogDebug("Need to spend gold...");
            // TODO: implement some way to spent gold.
            return Task.FromResult(0);
        }

        private async Task HandleDimensionMissionQuest(string questInfo, (bool Success, int Current, int Max) completionStatus, CancellationToken token)
        {
            if (questInfo.Similarity("[DIMENSION MISSION] Acquire Contribution Reward 1 time") > 0.8)
            {
                Logger.LogDebug("Trying to acquire contribution reward 1 time.");
                // TODO: Run dimension mission until we can get contribution reward 1 time.

                var dimensionMission = new DimensionMission(Game, Repository, Settings);
                dimensionMission.CollectRewardCount = 1;
                await dimensionMission.Run(token);
            }
            else if(questInfo.Similarity("[DIMENSION MISSION] Clear 10 times") > 0.8)
            {
                var dimensionMission = new DimensionMission(Game, Repository, Settings);
                // TODO: Add option to run specific amount of missions.
                dimensionMission.CollectRewardCount = 2;
                await dimensionMission.Run(token);
            }
        }

        private async Task TabToContinue(CancellationToken token)
        {
            if (Game.IsVisible(UIds.HEROIC_QUEST_TAB_THE_SCREEN_TO_CONTINUE))
            {
                Logger.LogDebug("Taping screen to continue.");
                Game.Click(UIds.HEROIC_QUEST_TAB_THE_SCREEN_TO_CONTINUE);
                await Task.Delay(1000, token);
            }
        }
    }
}
