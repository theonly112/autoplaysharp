using autoplaysharp.Contracts;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace autoplaysharp.Game.Tasks
{
    internal abstract class ContentStatusBoardDependenTask : GameTask
    {
        protected readonly IUiRepository Repository;
        private Dictionary<string, ContentStatus> _contentStatusList = new Dictionary<string, ContentStatus>();

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

        protected ContentStatusBoardDependenTask(IGame game, IUiRepository repository) : base(game)
        {
            Repository = repository;
        }

        public async Task<bool> UpdateContentStatusBoard()
        {
            if(!await GoToMainScreen())
            {
                Console.WriteLine("Failed to go to main screen");
                return false;
            }
            Game.Click("CONTENT_STATUS_BOARD_BUTTON");
            if(!await WaitUntilVisible("CONTENT_STATUS_BOARD_MENU_HEADER"))
            {
                Console.WriteLine("Failed to update content status board");
                return false;
            }
            _contentStatusList.Clear();

            // For now draging 1 times seems sufficent.
            for (int i = 0; i < 1; i++)
            {
                for (var col = 0; col < 3; col++)
                {
                    for (var row = 0; row < 4; row++)
                    {
                        var name = Game.GetText(Repository["CONTENT_STATUS_BOARD_ITEM_NAME_DYN", col, row]);
                        var status = Game.GetText(Repository["CONTENT_STATUS_BOARD_ITEM_STATUS_DYN", col, row]);
                        var isCompleted = Game.IsVisible(Repository["CONTENT_STATUS_BOARD_ITEM_NAME_COMPLETED_DYN", col, row]);
                        var statusEntry = new ContentStatus(name, isCompleted, status);
                        if (_contentStatusList.ContainsKey(name))
                            continue;

                        _contentStatusList.Add(name, statusEntry);
                        Console.WriteLine($"{name} - Compelted: {isCompleted} - Status text: {status}");
                    }
                }
                Game.Drag("CONTENT_STATUS_BOARD_DRAG_START", "CONTENT_STATUS_BOARD_DRAG_END");
                await Task.Delay(500);
            }

            Game.Click("CONTENT_STATUS_BOARD_GOTO_MAINSCREEN");
            await WaitUntilVisible("MAIN_MENU_ENTER");
            return true;
        }

        protected async Task<bool> StartContentBoardMission(string name)
        {
            if(!await UpdateContentStatusBoard())
            {
                return false;
            }
            // TODO refactor this ...
            if(!await WaitUntilVisible("MAIN_MENU_ENTER"))
            {
                Console.WriteLine("Cannot find enter button.. Not on main screen?");
                return false;
            }
            await Task.Delay(500);
            Game.Click("CONTENT_STATUS_BOARD_BUTTON");
            if (!await WaitUntilVisible("CONTENT_STATUS_BOARD_MENU_HEADER"))
            {
                Console.WriteLine("Failed to navigati to content status board");
                return false;
            }
            for (int i = 0; i < 1; i++)
            {
                for (var col = 0; col < 3; col++)
                {
                    for (var row = 0; row < 3; row++)
                    {
                        var element = Repository["CONTENT_STATUS_BOARD_ITEM_NAME_DYN", col, row];
                        var mission_name = Game.GetText(element);
                        if(mission_name == name)
                        {
                            Game.Click(element);
                            return true;
                        }
                    }
                }
                Game.Drag("CONTENT_STATUS_BOARD_DRAG_START", "CONTENT_STATUS_BOARD_DRAG_END");
                await Task.Delay(500);
            }
            return false;
        }

        protected ContentStatus GetMissionStatus(string id)
        {
            if (!_contentStatusList.ContainsKey(id))
            {
                return null;
            }
            return _contentStatusList[id];
        }


    }
}
