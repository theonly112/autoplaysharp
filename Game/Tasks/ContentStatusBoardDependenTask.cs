﻿using autoplaysharp.Contracts;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Vulkan.Wayland;

namespace autoplaysharp.Game.Tasks
{
    internal abstract class ContentStatusBoardDependenTask : GameTask
    {
        private readonly IUiRepository _repository;
        private Dictionary<string, ContentStatus> _contentStatusList = new Dictionary<string, ContentStatus>();

        protected class ContentStatus
        {
            internal static Regex StatusRegex = new Regex(@"(\d*)/(\d*)");
            public ContentStatus(string name, string status)
            {
                ContentName = name;
                Status = status;
            }

            public string ContentName { get; }
            public string Status { get; }
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
            _repository = repository;
        }

        public async Task UpdateContentStatusBoard()
        {
            await GoToMainScreen();
            Game.Click("CONTENT_STATUS_BOARD_BUTTON");
            if(!await WaitUntilVisible("CONTENT_STATUS_BOARD_MENU_HEADER"))
            {
                Console.WriteLine("Failed to update content status board");
                return;
            }
            _contentStatusList.Clear();

            // For now draging 3 times seems sufficent.
            for (int i = 0; i < 3; i++)
            {
                for (var col = 0; col < 3; col++)
                {
                    for (var row = 0; row < 3; row++)
                    {
                        var name = Game.GetText(_repository["CONTENT_STATUS_BOARD_ITEM_NAME_DYN", col, row]);
                        var status = Game.GetText(_repository["CONTENT_STATUS_BOARD_ITEM_STATUS_DYN", col, row]);
                        var statusEntry = new ContentStatus(name, status);
                        if (_contentStatusList.ContainsKey(name))
                            continue;

                        _contentStatusList.Add(name, statusEntry);
                        Console.WriteLine($"{name} - {status}");
                    }
                }
                Game.Drag("CONTENT_STATUS_BOARD_DRAG_START", "CONTENT_STATUS_BOARD_DRAG_END");
                await Task.Delay(500);
            }

            Game.Click("CONTENT_STATUS_BOARD_GOTO_MAINSCREEN");
            await Task.Delay(500);
        }

        protected async Task StartContentBoardMission(string name)
        {
            // TODO refactor this ...

            Game.Click("CONTENT_STATUS_BOARD_BUTTON");
            if (!await WaitUntilVisible("CONTENT_STATUS_BOARD_MENU_HEADER"))
            {
                Console.WriteLine("Failed to navigati to content status board");
                return;
            }
            for (int i = 0; i < 3; i++)
            {
                for (var col = 0; col < 3; col++)
                {
                    for (var row = 0; row < 3; row++)
                    {
                        var element = _repository["CONTENT_STATUS_BOARD_ITEM_NAME_DYN", col, row];
                        var mission_name = Game.GetText(element);
                        if(mission_name == name)
                        {
                            Game.Click(element);
                            return;
                        }
                    }
                }
                Game.Drag("CONTENT_STATUS_BOARD_DRAG_START", "CONTENT_STATUS_BOARD_DRAG_END");
                await Task.Delay(500);
            }
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
