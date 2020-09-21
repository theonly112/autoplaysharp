using autoplaysharp.Contracts.Configuration.Tasks;
using System.Collections.Generic;

namespace autoplaysharp.Contracts.Configuration
{
    public interface ISettings
    {
        string WindowName { get; set; }
        IList<string> RoutineItems { get; set; }
        EmulatorType EmulatorType { get; set; }
        ITimelineBattleSettings TimelineBattle { get; }
        IAllianceBattleSettings AllianceBattle { get; }
    }
}
