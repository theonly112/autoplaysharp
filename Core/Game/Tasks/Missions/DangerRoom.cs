using autoplaysharp.Contracts;
using System;
using System.Globalization;
using System.Linq;
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
            // TODO: retrive username...
            if(await StartContentBoardMission("DANGER ROOM") == null)
            {
                return;
            }
            if (!await WaitUntilVisible("DANGER_ROOM_START_NORMAL_MODE"))
            {
                return;
            }
            await Task.Delay(1000);
            Game.Click("DANGER_ROOM_START_NORMAL_MODE");
            if(!await WaitUntilVisible("DANGER_ROOM_LOBBY_HEADER"))
            {
                return;
            }

            await Task.Delay(2000); // What do we wait for here???

            // Re-using normal mode button for now... TODO: should be its own entry.
            Game.Click("DANGER_ROOM_START_NORMAL_MODE");
            await Task.Delay(2000); // What do we wait for here???

            await WaitForCharacterSelection();

            await SelectCharacter();

            // TODO: wait for game to start.

            if(!await WaitUntilVisible("DANGER_ROOM_WAITING_FOR_HEROES", 60, 0.2f))
            {
                Console.WriteLine("Start screen did not appear. Cancelling...");
                return;
            }

            Console.WriteLine("Game started... Starting fight bot.");
            // TODO: start battle bot.
            if(!await RunAutoFight(token))
            {
                Console.WriteLine("Failed to run fight bot...");
                return;
            }

            await Task.Delay(3000);

            Game.Click("DANGER_ROOM_ENDSCREEN_NEXT");

            await Task.Delay(5000);

            Game.Click("DANGER_ROOM_ENDSCREEN_HOME");

            await Task.Delay(5000);
        }

        private async Task<bool> RunAutoFight(CancellationToken token)
        {
            var autoFight = new AutoFight(Game, Repository, () => Game.IsVisible("DANGER_ROOM_HIGHEST_EXCLUSIV_SKILL_COUNT"));
            var autoFightTask = autoFight.Run(token);

            if (await Task.WhenAny(autoFightTask, Task.Delay(300 * 1000)) == autoFightTask)
            {
                await autoFightTask; // catch exception if thrown...
                return true;
            }

            return false;
        }

        internal async Task SelectCharacter()
        {
            var userNames = Enumerable.Range(0, 3).Select(x => Game.GetText(Repository["DANGER_ROOM_USER_NAME_DYN", x, 0])).ToArray();
            Console.WriteLine($"Usernames: {string.Join(",", userNames)}");
            var myUserName = "";
            Console.WriteLine("Waiting for our turn");
            var myPosition = Array.IndexOf(userNames, myUserName);
            await WaitUntilVisible(Repository["DANGER_ROOM_CURRENTLY_SELECTING_DYN", myPosition, 0], 30, 1);
            Console.WriteLine("Out turn. Selecting first availabe character");

            Regex percentageRegex = new Regex(@"(\d+.+)%");
            int bestX = 0, bestY = 0;
            float highestPercentage = 0f;
            for (var y = 0; y < 2; y++)
            {
                for (var x = 0; x < 3; x++)
                {
                    var text = Game.GetText(Repository["DANGER_ROOM_CHARACTER_PERCENTAGE_DYN", x, y]);
                    var match = percentageRegex.Match(text);
                    if(match.Success)
                    {
                        var percentageText = match.Groups[1].Value;
                        if (float.TryParse(percentageText, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture,out var percentage))
                        {
                            if(percentage > highestPercentage)
                            {
                                Console.WriteLine($"Found new best character. ({x}/{y})");
                                if(!Game.IsVisible(Repository["DANGER_ROOM_CHARACTER_ALREADY_SELECTED_DYN", x,y]))
                                {
                                    bestX = x;
                                    bestY = y;
                                    highestPercentage = percentage;
                                }
                                else
                                {
                                    Console.WriteLine("Unfortunately character already taken");
                                }

                            }
                        }
                    }
                }
            }

            Game.Click(Repository["DANGER_ROOM_CHARACTER_PERCENTAGE_DYN", bestX, bestY]);

        }

        private async Task WaitForCharacterSelection()
        {
            bool waiting = true;
            while (waiting)
            {
                var searchingForTeam = WaitUntilVisible("DANGER_ROOM_SEARCHING_FOR_TEAM", 10, 1);
                var searchingForOpponent = WaitUntilVisible("DANGER_ROOM_SEARCHING_FOR_OPPONENT", 10, 1);
                var characterSelection = WaitUntilVisible("DANGER_ROOM_CHARACTER_SELECTION_HEADER", 10, 1);

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
                        Console.WriteLine("None of the expected elements found...");
                        return;
                    }
                    else
                    {
                        Console.WriteLine(completed == searchingForTeam ? "Searching for team" : "Searching for opponent");
                        await Task.Delay(2000);
                    }
                }
            }
        }
    }
}
