using System;
using System.Threading;
using System.Threading.Tasks;
using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;
using autoplaysharp.Contracts.Errors;
using Microsoft.Extensions.Logging;

namespace autoplaysharp.Core.Game.Tasks.Missions
{
    public class AllianceBattle : ContentStatusBoardDependenTask
    {
        private const string MissionName = "ALLIANCE BATTLE";

        public AllianceBattle(IGame game, IUiRepository repository, ISettings settings) : base(game, repository, settings)
        {
            ShouldRunExtremeMode = settings.AllianceBattle.RunExtremeMode;
            ShouldRunNormalMode = settings.AllianceBattle.RunNormalMode;
        }

        private bool ShouldRunNormalMode { get; }
        private bool ShouldRunExtremeMode { get; }
        
        protected override async Task RunCore(CancellationToken token)
        {
            if(ShouldRunNormalMode)
            {
                await RunNormalMode(token);
            }

            if(ShouldRunExtremeMode)
            {
                await RunExtremeMode(token);
            }
        }

        private async Task RunExtremeMode(CancellationToken token)
        {
            if (await StartContentBoardMission(MissionName) == null)
            {
                Logger.LogError($"Failed to go to {MissionName}");
                return;
            }
            await WaitUntilVisible(UIds.ALLIANCE_BATTLE_MODE_HEADER, token);

            if (!await WaitUntilVisible(UIds.ALLIANCE_BATTLE_EXTREME_MODE_READY, token))
            {
                Logger.LogError("Extreme mode not available.");
                return;
            }

            await Task.Delay(1000, token);

            Game.Click(UIds.ALLIANCE_BATTLE_EXTREME_MODE_READY);

            await SelectHeroes();

            Game.Click(UIds.ALLIANCE_BATTLE_EXTREME_MODE_START);

            await Task.Delay(1000, token);

            if (Game.IsVisible(UIds.ALLIANCE_BATTLE_NOTICE_THREE_CHARACTERS_REQUIRED))
            {
                Game.Click(UIds.GENERIC_MISSION_NOTICE_DISCONNECTED_OK);
                await Task.Delay(1000, token);
                await SelectHeroes();
                await Task.Delay(1000, token);
                Game.Click(UIds.ALLIANCE_BATTLE_EXTREME_MODE_START);
            }

            if (!await HandleStartNotices())
            {
                Logger.LogError("Failed to start mission...");
                return;
            }

            if(!await RunAutoFight(token))
            {
                Logger.LogError("Failed to run autofight");
                return;
            }

            await Task.Delay(1000, token);

            if(!await ClickWhenVisible(UIds.ALLIANCE_BATTLE_END_SCREEN_HOME))
            {
                Game.OnError(new ElementNotFoundError(Repository[UIds.ALLIANCE_BATTLE_END_SCREEN_HOME]));
                return;
            }

            await HandleHeroicQuestNotice();
        }

        internal async Task RunNormalMode(CancellationToken token)
        {
            if (await StartContentBoardMission(MissionName) == null)
            {
                Logger.LogError($"Failed to go to {MissionName}");
                return;
            }
            await WaitUntilVisible(UIds.ALLIANCE_BATTLE_MODE_HEADER, token);

            if (!await WaitUntilVisible(UIds.ALLIANCE_BATTLE_NORMAL_MODE_READY, token))
            {
                Logger.LogError("Normal mode not available.");
                return;
            }

            await Task.Delay(1000, token);

            Game.Click(UIds.ALLIANCE_BATTLE_NORMAL_MODE_READY);


            if (!await WaitUntilVisible(UIds.ALLIANCE_BATTLE_NORMAL_MODE_START, token))
            {
                Logger.LogError("Normal mode start button not available.");
                return;
            }

            await SelectHeroes();

            Game.Click(UIds.ALLIANCE_BATTLE_NORMAL_MODE_START);

            await Task.Delay(1000, token);

            if (Game.IsVisible(UIds.ALLIANCE_BATTLE_NOTICE_THREE_CHARACTERS_REQUIRED))
            {
                Game.Click(UIds.GENERIC_MISSION_NOTICE_DISCONNECTED_OK);
                await Task.Delay(1000, token);
                await SelectHeroes();
                await Task.Delay(1000, token);
                Game.Click(UIds.ALLIANCE_BATTLE_NORMAL_MODE_START);
            }

            if (!await HandleStartNotices())
            {
                Logger.LogError("Failed to start mission...");
                return;
            }

            var autoFight = new AutoFight(Game, Repository, Settings);
            await autoFight.Run(token);

            await Task.Delay(1000, token);

            Game.Click(UIds.ALLIANCE_BATTLE_END_SCREEN_HOME);

            await Task.Delay(1000, token);

            await HandleHeroicQuestNotice();
        }

        private async Task SelectHeroes()
        {
            if (!await WaitUntilVisible(UIds.ALLIANCE_BATTLE_HERO_SELECTION_HEADER))
            {
                Logger.LogError("Failed: Hero selection screen did not appear");
                return;
            }

            for (int i = 0; i < 3; i++)
            {
                Game.Click(Repository[UIds.ALLIANCE_BATTLE_HERO_SELECTION_DYN, i, 0]);
                await Task.Delay(200);
            }
        }

        private async Task<bool> RunAutoFight(CancellationToken token)
        {
            Func<bool> died = () => Game.IsVisible(UIds.ALLIANCE_BATTLE_SELECT_NEW_CHARACTER_AND_CONTINUE);
            Func<bool> battleEnded = () => Game.IsVisible(UIds.ALLIANCE_BATTLE_ENDED_MESSAGE);
            Func<bool> cleared = () => Game.IsVisible(UIds.ALLIANCE_BATTLE_CLEAR_MESSAGE);
            Func<bool> threeCharsRequired = () => Game.IsVisible(UIds.ALLIANCE_BATTLE_NOTICE_THREE_CHARACTERS_REQUIRED);

            var autoFight = new AutoFight(Game, Repository, Settings, died, battleEnded, cleared, threeCharsRequired);
            var autoFightTask = autoFight.Run(token);

            // TODO: is timeout fallback even necessary?
            if(await Task.WhenAny(autoFightTask, Task.Delay(300 * 1000, token)) == autoFightTask)
            {
                await autoFightTask; // catch exception if thrown...

                if(Game.IsVisible(UIds.ALLIANCE_BATTLE_SELECT_NEW_CHARACTER_AND_CONTINUE))
                {
                    Game.Click(UIds.ALLIANCE_BATTLE_SELECT_NEW_CHARACTER_AND_CONTINUE_CANCEL);
                    await Task.Delay(2000, token);
                }

                return true;
            }

            return false;
        }
    }
}
