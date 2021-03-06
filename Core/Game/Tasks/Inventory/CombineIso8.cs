﻿using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace autoplaysharp.Core.Game.Tasks.Inventory
{
    internal class CombineIso8 : GameTask
    {
        public CombineIso8(IGame game, IUiRepository repository, ISettings settings) : base(game, repository, settings)
        {
        }

        protected override async Task RunCore(CancellationToken token)
        {
            await GoToMainScreen();

            Logger.LogError("Not implemented yet");

            // TODO: implement.

            // handles heroic quest notices.
            await GoToMainScreen();
        }
    }
}
