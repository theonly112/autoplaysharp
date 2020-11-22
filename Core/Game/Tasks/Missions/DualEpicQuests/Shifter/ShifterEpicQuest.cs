using System.Threading.Tasks;
using autoplaysharp.Contracts;
using autoplaysharp.Contracts.Configuration;
using autoplaysharp.Contracts.Errors;
using Microsoft.Extensions.Logging;

namespace autoplaysharp.Core.Game.Tasks.Missions.DualEpicQuests.Shifter
{
    public abstract class ShifterEpicQuest : GenericDualEpicQuest
    {
        protected ShifterEpicQuest(IGame game, IUiRepository repository, ISettings settings) : base(game, repository, settings)
        {
        }

        protected override async Task<bool> OnGameStart()
        {
            if (!Settings.EpicQuest.RestartForBioFarming)
            {
                return true;
            }
            
            var element = Repository[UIds.EPIC_QUEST_SHIFTER_APPEARED];
            if (await WaitUntilVisible(element, 15))
            {
                return true;
            }
            
            Logger.LogInformation("Shifter did not appear. Restarting game!");
            try
            {
                Game.OnError(new ElementNotFoundError(element));
            }
            catch
            {
                // ignored
                // We specifically do not want to exit with a exception here.
                // We know how to continue. (TODO: argument for "ShouldThrow?")
            }

            return false;

        }
    }
}
