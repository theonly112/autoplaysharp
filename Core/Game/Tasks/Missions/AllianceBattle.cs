using autoplaysharp.Contracts;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace autoplaysharp.Game.Tasks.Missions
{
    public class AllianceBattle : ContentStatusBoardDependenTask
    {
        private const string MissionName = "ALLIANCE BATTLE";

        public AllianceBattle(IGame game, IUiRepository repository) : base(game, repository)
        {
        }

        protected override async Task RunCore(CancellationToken token)
        {
            await RunNormalMode(token);
            await RunExtremeMode(token);
        }

        private async Task RunExtremeMode(CancellationToken token)
        {
            if (await StartContentBoardMission(MissionName) == null)
            {
                Logger.LogError($"Failed to go to {MissionName}");
                return;
            }
            await WaitUntilVisible(UIds.ALLIANCE_BATTLE_MODE_HEADER);

            if (!await WaitUntilVisible(UIds.ALLIANCE_BATTLE_EXTREME_MODE_READY))
            {
                Logger.LogError("Extreme mode not available.");
                return;
            }

            await Task.Delay(1000);

            Game.Click(UIds.ALLIANCE_BATTLE_EXTREME_MODE_READY);

            await SelectHeroes();

            Game.Click(UIds.ALLIANCE_BATTLE_EXTREME_MODE_START);

            await Task.Delay(1000);

            if (Game.IsVisible(UIds.ALLIANCE_BATTLE_NOTICE_THREE_CHARACTERS_REQUIRED))
            {
                Game.Click(UIds.GENERIC_MISSION_NOTICE_DISCONNECTED_OK);
                await Task.Delay(1000);
                await SelectHeroes();
                await Task.Delay(1000);
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

            await Task.Delay(1000);

            Game.Click(UIds.ALLIANCE_BATTLE_END_SCREEN_HOME);

            await Task.Delay(5000);
        }

        internal async Task RunNormalMode(CancellationToken token)
        {
            if (await StartContentBoardMission(MissionName) == null)
            {
                Logger.LogError($"Failed to go to {MissionName}");
                return;
            }
            await WaitUntilVisible(UIds.ALLIANCE_BATTLE_MODE_HEADER);

            if (!await WaitUntilVisible(UIds.ALLIANCE_BATTLE_NORMAL_MODE_READY, token))
            {
                Logger.LogError("Normal mode not available.");
                return;
            }

            await Task.Delay(1000);

            Game.Click(UIds.ALLIANCE_BATTLE_NORMAL_MODE_READY);


            if (!await WaitUntilVisible(UIds.ALLIANCE_BATTLE_NORMAL_MODE_START, token))
            {
                Logger.LogError("Normal mode start button not available.");
                return;
            }

            await SelectHeroes();

            Game.Click(UIds.ALLIANCE_BATTLE_NORMAL_MODE_START);

            await Task.Delay(1000);

            if (Game.IsVisible(UIds.ALLIANCE_BATTLE_NOTICE_THREE_CHARACTERS_REQUIRED))
            {
                Game.Click(UIds.GENERIC_MISSION_NOTICE_DISCONNECTED_OK);
                await Task.Delay(1000);
                await SelectHeroes();
                await Task.Delay(1000);
                Game.Click(UIds.ALLIANCE_BATTLE_NORMAL_MODE_START);
            }

            if (!await HandleStartNotices())
            {
                Logger.LogError("Failed to start mission...");
                return;
            }

            var autoFight = new AutoFight(Game, Repository);
            await autoFight.Run(token);

            await Task.Delay(1000);

            Game.Click(UIds.ALLIANCE_BATTLE_END_SCREEN_HOME);

            await Task.Delay(5000);
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
            var autoFight = new AutoFight(Game, Repository, () => Game.IsVisible(UIds.ALLIANCE_BATTLE_ENDED_MESSAGE), () => Game.IsVisible(UIds.ALLIANCE_BATTLE_CLEAR_MESSAGE));
            var autoFightTask = autoFight.Run(token);

            if(await Task.WhenAny(autoFightTask, Task.Delay(300*1000)) == autoFightTask)
            {
                await autoFightTask; // catch exception if thrown...
                return true;
            }

            return false;
        }
    }
}
