using autoplaysharp.Contracts;
using F23.StringSimilarity;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace autoplaysharp.Game.Tasks
{
    public abstract class ContentStatusBoardDependenTask : GameTask
    {
        protected class ContentStatus
        {
            internal static Regex StatusRegex = new Regex(@"(\d*)/(\d*)");

            public ContentStatus(string name, bool completed, string status)
            {
                ContentName = name;
                Completed = completed;
                Status = status;
            }


            public string ContentName { get; }
            public string Status { get; }
            public bool Completed { get; }
            public int Used
            {
                get
                {
                    return GetValue(2);
                }
            }

            public int Available 
            {
                get
                {
                    return GetValue(1);
                }
            }

            private int GetValue(int i)
            {
                var match = StatusRegex.Match(Status);
                if (!match.Success)
                {
                    return -1;
                }
                // TODO parse string
                var v = match.Groups[i].Value;
                if (!int.TryParse(v, out var u))
                {
                    return -1;
                }
                return u;
            }
        }

        protected ContentStatusBoardDependenTask(IGame game, IUiRepository repository) : base(game, repository)
        {
        }

        protected async Task<ContentStatus> StartContentBoardMission(string name)
        {
            if(!await GoToMainScreen())
            {
                Logger.LogError("Couldn't go to main screen.");
            }

            if(!await WaitUntilVisible("MAIN_MENU_ENTER"))
            {
                Logger.LogError("Cannot find enter button.. Not on main screen?");
                return null;
            }

            await Task.Delay(500);
            Game.Click("CONTENT_STATUS_BOARD_BUTTON");
            if (!await WaitUntilVisible("CONTENT_STATUS_BOARD_MENU_HEADER"))
            {
                Logger.LogError("Failed to navigate to content status board");
                return null;
            }

            await Task.Delay(500);

            for (int i = 0; i < 5; i++)
            {
                for (var col = 0; col < 3; col++)
                {
                    for (var row = 0; row < 12; row++)
                    {
                        var nameElement = Repository["CONTENT_STATUS_BOARD_ITEM_NAME_DYN", col, row];
                        var status = Game.GetText(Repository["CONTENT_STATUS_BOARD_ITEM_STATUS_DYN", col, row]);
                        var isCompleted = Game.IsVisible(Repository["CONTENT_STATUS_BOARD_ITEM_NAME_COMPLETED_DYN", col, row]);
                        var statusEntry = new ContentStatus(name, isCompleted, status);
                        var mission_name = Game.GetText(nameElement);

                        var nl = new NormalizedLevenshtein();
                        var similarity = nl.Similarity(name, mission_name);
                        if (similarity >= 0.8) // 80% should be fine. names are different enough.
                        {
                            Logger.LogDebug($"Clicking on element because it matches expected: {name} actual: {mission_name} similarity: {similarity}");
                            Game.Click(nameElement);

                            await Task.Delay(1000); // waiting briefly for page to change.

                            if (Game.IsVisible(UIds.GENERIC_MISSION_CUSTOM_OFFER_FOR_AGENTS))
                            {
                                Game.Click(UIds.GENERIC_MISSION_CUSTOM_OFFER_FOR_AGENTS_CLOSE);
                                await Task.Delay(1000);
                            }

                            return statusEntry;
                        }
                        else
                        {
                            Logger.LogDebug($"Found mission {mission_name}. But its not what we are looking for. Similarity {similarity}");
                        }
                    }
                }
                Game.Drag("CONTENT_STATUS_BOARD_DRAG_START", "CONTENT_STATUS_BOARD_DRAG_END");
            }
            return null;
        }
    }
}
