using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace autoplaysharp.Core.Game.Tasks.Missions.DeluxeEpicQuests
{
    public abstract class GenericDeluxeEpicQuest : GenericEpicQuest
    {
        protected GenericDeluxeEpicQuest(IGame game, IUiRepository repository, ISettings settings) : base(game, repository, settings)
        {
        }

        protected abstract string MissionName { get; }

        protected override async Task RunCore(CancellationToken token)
        {
            var status = await StartContentBoardMission(MissionName);
            if(status == null)
            {
                Logger.LogError($"Could not find mission {MissionName}. Maybe it's still locked");
                return;
            }

            if (Settings.EpicQuest.UseClearTickets && status.Available > 0)
            {
                await UseClearTickets();
            }
            else
            {
                for (int i = 0; i < status.Available; i++)
                {
                    await RunMissionCore(token);

                    await StartContentBoardMission(MissionName);
                }
            }
            

            await Task.Delay(2000, token);
            await GoToMainScreen(token);
        }
    }
}
