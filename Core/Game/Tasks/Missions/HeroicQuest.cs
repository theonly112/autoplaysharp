using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;
using autoplaysharp.Core.Game.Tasks.Inventory;
using autoplaysharp.Core.Helper;
using Microsoft.Extensions.Logging;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using autoplaysharp.Contracts.Errors;

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
                Game.OnError(new ElementNotFoundError(Repository[UIds.HEROIC_QUEST_HEADER]));
                return;
            }

            Game.Click(UIds.HEROIC_QUEST_HEADER);

            await Task.Delay(1000, token);

            if(Game.IsVisible(UIds.HEROIC_QUEST_WORLD_MAP))
            {
                Logger.LogError("World map appeared when starting heroic quest. Don't know how to proceed.");
                Game.Click(UIds.HEROIC_QUEST_WORLD_MAP_OK);
                await Task.Delay(1000);
                await GoToMainScreen(token);
                return;
            }

            if(Game.IsVisible(UIds.HEROIC_QUEST_CYSTAL_CHEST_NOTICE))
            {
                Logger.LogDebug("Closing crystal chest notice.");
                Game.Click(UIds.HEROIC_QUEST_CYSTAL_CHEST_NOTICE_CANCEL);
                await Task.Delay(1000, token);

                if (Game.IsVisible(UIds.HEROIC_QUEST_CYSTAL_CHEST_FULL_NOTICE))
                {
                    Logger.LogDebug("Clsing crytsal chest full notice.");
                    Game.Click(UIds.HEROIC_QUEST_CYSTAL_CHEST_FULL_NOTICE_OK);
                    await Task.Delay(1000, token);
                }
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
                case var _ when questInfo.StartsWith("[DIMENSION MISSION]"):
                    await HandleDimensionMissionQuest(questInfo, token);
                    break;
                case var _ when new Regex(@"Use .* Gold").IsMatch(questInfo):
                    await HandleUseGoldMission(token);
                    break;
                case var _ when questInfo.StartsWith("[ALLIANCE BATTLE]"):
                    await HandleAllianceBattleQuest(questInfo, token);
                    break;
                case var _ when questInfo.StartsWith("[CUSTOM GEAR]"):
                    await HandleCustomGearQuest(questInfo, token);
                    break;
                case var _ when questInfo.StartsWith("[ENCHANTED URU]"):
                    await HandleUruQuest(questInfo, token);
                    break;
                case var _ when questInfo.StartsWith("Beat World Boss"):
                    await HandleWorldBossQuest(questInfo);
                    break;
                case var _ when questInfo.StartsWith("[ISO-8]"):
                    await HandleIso8Quest(questInfo, token);
                    break;
                case var _ when questInfo.StartsWith("[DANGER ROOM"):
                    await HandleDangerRoomQuest(questInfo, token);
                    break;
                case var _ when new Regex(@"Use .* Energy").IsMatch(questInfo):
                    await HandleUseEnergQuest(token);
                    break;
                case var _ when questInfo.StartsWith("[LEGENDARY BATTLE]"):
                    await HandleLegendaryBattleQuest(completionStatus, token);
                    break;
                case var _ when questInfo.StartsWith("[TIMELINE BATTLE"):
                    await HandleTimelineBattle(token);
                    break;
                case var _ when questInfo.StartsWith("[WORLD BOSS INVASION]"):
                    await HandleWorldBossInvasionQuest(questInfo, token);
                    break;
                case var _ when questInfo.StartsWith("[CARD]"):
                    await HandleComicCardQuest(questInfo, token);
                    break;
                case var _ when questInfo.StartsWith("[CO-OP PLAY]"):
                    await HandleCoopQuest(questInfo, token);
                    break;
                case var _ when questInfo.StartsWith("[HEROIC QUEST]"):
                    await HandleHeroicQuestEndFight(token);
                    break;
                default:
                    Logger.LogError($"Unhandled heroic quest: {questInfo}");
                    return;
            }

            if (token.IsCancellationRequested)
                return;

            // TODO: right here we could loop.
            await RunCore(token);
        }

        private async Task HandleHeroicQuestEndFight(CancellationToken token)
        {
            await ClickWhenVisible(UIds.HEROIC_QUEST_QUEST_INFO_GOTO);
            await Task.Delay(2000);
            await ClickWhenVisible(UIds.HEROIC_QUEST_END_QUEST_START);
            await ClickWhenVisible(UIds.HEROIC_QUEST_END_QUEST_SKIP);

            Func<bool> homeVisible = () => Game.IsVisible(UIds.HEROIC_QUEST_END_QUEST_HOME);
            var authFight = new AutoFight(Game, Repository, Settings, homeVisible);
            await authFight.Run(token);
            await Task.Delay(1000);
            await ClickWhenVisible(UIds.HEROIC_QUEST_END_QUEST_HOME);
            await ClickWhenVisible(UIds.HEROIC_QUEST_END_QUEST_SKIP);
            await HandleHeroicQuestNotice();
            await GoToMainScreen();
        }

        private async Task HandleCoopQuest(string questInfo, CancellationToken token)
        {
            if(questInfo.Contains("Acquire"))
            {
                var coop = new CoopMission(Game, Repository, Settings);
                coop.RewardCount = 2; //TODO: are there other quests like this.
                await coop.Run(token);
            }
        }

        private async Task HandleComicCardQuest(string questInfo, CancellationToken token)
        {
            if(questInfo.Contains("Upgrade"))
            {
                var upgrade = new UpgradeCards(Game, Repository, Settings);
                await upgrade.Run(token);
            }
        }

        private async Task HandleWorldBossInvasionQuest(string questInfo, CancellationToken token)
        {
            if (questInfo.Contains("Complete"))
            {
                var wbi = new WorldBossInvasion(Game, Repository, Settings);
                await wbi.Run(token);
            }

            if(questInfo.Contains("Participate"))
            {
                var wbi = new WorldBossInvasion(Game, Repository, Settings);
                await wbi.Run(token);
            }
        }

        private async Task HandleTimelineBattle(CancellationToken token)
        {
            var timelineBattle = new TimelineBattle(Game, Repository, Settings);
            // TODO: settings?!?!?!
            await timelineBattle.Run(token);
        }

        private async Task HandleLegendaryBattleQuest((bool Success, int Current, int Max) completionStatus, CancellationToken token)
        {
            Logger.LogDebug("Running legendary battle to complete heroic quest.");
            var legendaryBattle = new LegendaryBattle(Game, Repository, Settings);
            legendaryBattle.ClearCount = completionStatus.Max - completionStatus.Current;
            await legendaryBattle.Run(token);
        }

        private async Task HandleUseEnergQuest(CancellationToken token)
        {
            // TODO: let user select mission type to spend energy?
            Logger.LogDebug("Running dimension missions to spend energy");
            var dimensionMission = new DimensionMission(Game, Repository, Settings);
            dimensionMission.CollectRewardCount = 2; // 1 is not enough when boost points available.
            await dimensionMission.Run(token);
        }

        private async Task HandleDangerRoomQuest(string questInfo, CancellationToken token)
        {
            if(questInfo.Contains("Participate"))
            {
                // Text like: "[DANGER ROOM] Participate 1 time
                var dangerRoom = new DangerRoom(Game, Repository, Settings);
                dangerRoom.SingleRun = true;
                await dangerRoom.Run(token);
            }
        }

        private async Task HandleIso8Quest(string questInfo, CancellationToken token)
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

        private Task HandleWorldBossQuest(string questInfo)
        {
            if(questInfo.Contains("Participate"))
            {
                // TODO: dont really run world boss. just exit immediatly.
            }

            // TODO: run world boss...
            return Task.CompletedTask;
        }

        private async Task HandleUruQuest(string questInfo, CancellationToken token)
        {
            if(questInfo.Similarity("[ENCHANTED URU] Combine 3 times") > 0.8)
            {
                var upgradeUru = new CombineUru(Game, Repository, Settings);
                await upgradeUru.Run(token);
            }
        }

        private async Task HandleCustomGearQuest(string questInfo, CancellationToken token)
        {
            if (questInfo.Similarity("[CUSTOM GEAR] Upgrade x2") > 0.80)
            {
                var upgradeCustomGear = new UpgradeCustomGear(Game, Repository, Settings);
                await upgradeCustomGear.Run(token);
            }
        }

        private async Task HandleAllianceBattleQuest(string questInfo, CancellationToken token)
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

        private async Task HandleUseGoldMission(CancellationToken token)
        {
            // TODO: improve this...
            for (int i = 0; i < 3; i++)
            {
                // Set some settings to make sure enough, but not too much gold is spent...
                var combineUru = new CombineUru(Game, Repository, Settings);
                await combineUru.Run(token);
            }
        }

        private async Task HandleDimensionMissionQuest(string questInfo, CancellationToken token)
        {
            if (questInfo.Similarity("[DIMENSION MISSION] Acquire Contribution Reward 1 time") > 0.8)
            {
                Logger.LogDebug("Trying to acquire contribution reward 1 time.");
                var dimensionMission = new DimensionMission(Game, Repository, Settings);
                dimensionMission.CollectRewardCount = 1;
                await dimensionMission.Run(token);
            }
            else if(questInfo.Similarity("[DIMENSION MISSION] Clear 10 times") > 0.8)
            {
                Logger.LogDebug("Running 10 dimension missions");
                var dimensionMission = new DimensionMission(Game, Repository, Settings)
                {
                    Mode = DimensionMission.DimensionMissionRunMode.RunXMissions,
                    MissionCount = 10
                };
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
