using autoplaysharp.Contracts;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace autoplaysharp.Core.Game.Tasks.Missions.DeluxeEpicQuests
{
    public abstract class GenericDeluxeEpicQuest : GenericEpicQuest
    {
        public GenericDeluxeEpicQuest(IGame game, IUiRepository repository) : base(game, repository)
        {
        }

        protected abstract string MissionName { get; }

        protected override async Task RunCore(CancellationToken token)
        {
            var status = await StartContentBoardMission(MissionName);
            if(status == null)
            {
                Console.WriteLine($"Could not find mission {MissionName}. Maybe it's still locked");
                return;
            }

            for (int i = 0; i < status.Available; i++)
            {
                await RunMissionCore();

                await StartContentBoardMission(MissionName);
            }

            await Task.Delay(2000);
            await GoToMainScreen();
        }
    }
}
