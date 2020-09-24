﻿using autoplaysharp.Contracts;
using autoplaysharp.Game.Tasks;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace autoplaysharp.Core.Game.Tasks.Missions
{
    internal class WorldBoss : ContentStatusBoardDependenTask
    {
        public WorldBoss(IGame game, IUiRepository repository) : base(game, repository)
        {
        }

        protected override async Task RunCore(CancellationToken token)
        {
            var status = await StartContentBoardMission("WORLD BOSS");
            if(status.Completed)
            {
                Logger.LogInformation("World Boss already completed");
                return;
            }

            await Task.Delay(1000);
            // TODO: implement

            if(!await GoToMainScreen())
            {
                Logger.LogError("Failed to go back to main screen");
            }
        }
    }
}
