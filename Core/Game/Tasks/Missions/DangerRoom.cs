using autoplaysharp.Contracts;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace autoplaysharp.Game.Tasks.Missions
{
    public class DangerRoom : ContentStatusBoardDependenTask
    {
        public DangerRoom(IGame game, IUiRepository repository) : base(game, repository)
        {
        }

        protected override async Task RunCore(CancellationToken token)
        {
            if(await StartContentBoardMission("DANGER ROOM") == null)
            {
                return;
            }
            if (!await WaitUntilVisible(UIds.DANGER_ROOM_EXTREMEL_MODE))
            {
                Logger.LogError("Could not find extreme mode selection");
                return;
            }
            
            await Task.Delay(1000, token);
            Game.Click(UIds.DANGER_ROOM_EXTREMEL_MODE);

            await Task.Delay(500, token);
            Game.Click(UIds.DANGER_ROOM_ENTER_DANGER_ROOM);

            
            if(!await WaitUntilVisible(UIds.DANGER_ROOM_EXTREME_MODE_LOBBY_HEADER))
            {
                return;
            }

            await Task.Delay(2000, token); // What do we wait for here???

            // Re-using normal mode button for now... TODO: should be its own entry.
            Game.Click(UIds.DANGER_ROOM_JOIN_BUTTON);
            await Task.Delay(2000, token); // What do we wait for here???

            await WaitForCharacterSelection();

            await SelectCharacter();

            if(!await WaitUntilVisible(UIds.DANGER_ROOM_WAITING_FOR_HEROES, 60, 0.2f))
            {
                if (Game.IsVisible(UIds.DANGER_ROOM_GAME_CANCELED_NOTICE))
                {
                    Game.Click(UIds.DANGER_ROOM_GAME_CANCELED_NOTICE_OK);
                    await Task.Delay(2000, token);
                    Logger.LogError("Game was cancelled. Restarting");
                    await RunCore(token);
                }
                else
                {
                    Logger.LogError("Start screen did not appear. Cancelling...");
                }
                return;
            }

            Logger.LogInformation("Game started... Starting fight bot.");
            // TODO: start battle bot.
            if(!await RunAutoFight(token))
            {
                Logger.LogError("Failed to run fight bot...");
                return;
            }

            await Task.Delay(3000, token);

            Game.Click(UIds.DANGER_ROOM_ENDSCREEN_NEXT);

            await Task.Delay(5000, token);

            Game.Click(UIds.DANGER_ROOM_ENDSCREEN_HOME);

            await Task.Delay(5000, token);
        }

        private async Task<bool> RunAutoFight(CancellationToken token)
        {
            var autoFight = new AutoFight(Game, Repository, () => Game.IsVisible(UIds.DANGER_ROOM_HIGHEST_EXCLUSIV_SKILL_COUNT));
            var autoFightTask = autoFight.Run(token);

            if (await Task.WhenAny(autoFightTask, Task.Delay(300 * 1000, token)) == autoFightTask)
            {
                await autoFightTask; // catch exception if thrown...
                return true;
            }

            return false;
        }

        internal async Task SelectCharacter()
        {
            Regex percentageRegex = new Regex(@"(\d+.+)%");
            int bestX = 0, bestY = 0;
            float highestPercentage = 0f;
            for (var y = 0; y < 2; y++)
            {
                for (var x = 0; x < 3; x++)
                {
                    var percentageElement = Repository[UIds.DANGER_ROOM_CHARACTER_PERCENTAGE_DYN, x, y];
                    var text = Game.GetText(percentageElement);
                    var match = percentageRegex.Match(text);
                    if(match.Success)
                    {
                        var percentageText = match.Groups[1].Value;
                        if (float.TryParse(percentageText, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture,out var percentage))
                        {
                            if(percentage > highestPercentage)
                            {
                                Logger.LogDebug($"Found new best character. ({x}/{y})");
                                Game.Click(percentageElement);
                                await Task.Delay(250);
                                if (Game.IsVisible(UIds.DANGER_ROOM_CHARACTER_REWARD_AVAILABLE))
                                {
                                    bestX = x;
                                    bestY = y;
                                    highestPercentage = percentage;
                                }
                                else
                                {
                                    Logger.LogDebug("Unfortunately character already taken");
                                }
                            }
                        }
                    }
                }
            }

            Game.Click(Repository[UIds.DANGER_ROOM_CHARACTER_PERCENTAGE_DYN, bestX, bestY]);

        }

        private async Task WaitForCharacterSelection()
        {
            bool waiting = true;
            while (waiting)
            {
                var searchingForTeam = WaitUntilVisible(UIds.DANGER_ROOM_SEARCHING_FOR_TEAM, 5, 1);
                var searchingForOpponent = WaitUntilVisible(UIds.DANGER_ROOM_SEARCHING_FOR_OPPONENT, 5, 1);
                var characterSelection = WaitUntilVisible(UIds.DANGER_ROOM_CHARACTER_SELECTION_HEADER, 5, 1);

                var completed = await Task.WhenAny(searchingForTeam, searchingForOpponent, characterSelection);
                if (completed == characterSelection)
                {
                    var onCharacterSelectionScreen = await completed;
                    if (onCharacterSelectionScreen)
                    {
                        waiting = false;
                    }
                }
                else
                {
                    if (!await completed)
                    {
                        Logger.LogError("None of the expected elements found...");
                        return;
                    }
                    else
                    {
                        Logger.LogDebug(completed == searchingForTeam ? "Searching for team" : "Searching for opponent");
                        await Task.Delay(2000);
                    }
                }
            }
        }
    }
}
