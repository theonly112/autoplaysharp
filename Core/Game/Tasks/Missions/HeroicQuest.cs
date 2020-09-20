using autoplaysharp.Contracts;
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
        public HeroicQuest(IGame game, IUiRepository repository) : base(game, repository)
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

            if (Game.IsVisible(UIds.HEROIC_QUEST_TAB_THE_SCREEN))
            {
                Logger.LogDebug("Taping screen to continue.");
                Game.Click(UIds.HEROIC_QUEST_TAB_THE_SCREEN);
                await Task.Delay(1000, token);
            }

            if(Game.IsVisible(UIds.HEROIC_QUEST_QUEST_INFO_ACQUIRE))
            {
                Logger.LogDebug("Attempting to acquire heroic quest reward.");
                Game.Click(UIds.HEROIC_QUEST_QUEST_INFO_ACQUIRE);
                await Task.Delay(1000, token);
                await TabToContinue(token);

            }

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
            }

            await Task.Delay(1000);
            await ClickWhenVisible(UIds.HEROIC_QUEST_FINISHED_NOTICE);
            await Task.Delay(2000);
            await ClickWhenVisible(UIds.HEROIC_QUEST_QUEST_INFO_ACQUIRE);
            await Task.Delay(1000);
            Game.Click(UIds.HEROIC_QUEST_TAB_THE_SCREEN);

            // TODO: right here we could loop.

        }

        private async Task HandleCustomGearQuest(string questInfo, (bool Success, int Current, int Max) completionStatus, CancellationToken token)
        {
            if (questInfo.Similarity("[CUSTOM GEAR] Upgrade x2") > 0.80)
            {
                // TODO: implement custom gear upgrade.
            }
        }

        private async Task HandleAllianceBattleQuest(string questInfo, (bool Success, int Current, int Max) completionStatus, CancellationToken token)
        {
            if(questInfo.Similarity("[ALLIANCE BATTLE] Participate in Normal Mode") > 0.80)
            {
                var allianceBattle = new AllianceBattle(Game, Repository);
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

                var dimensionMission = new DimensionMission(Game, Repository);
                await dimensionMission.Run(token);
            }
        }

        private async Task TabToContinue(CancellationToken token)
        {
            if (Game.IsVisible(UIds.HEROIC_QUEST_TAB_THE_SCREEN))
            {
                Logger.LogDebug("Taping screen to continue.");
                Game.Click(UIds.HEROIC_QUEST_TAB_THE_SCREEN);
                await Task.Delay(1000, token);
            }
        }
    }
}
