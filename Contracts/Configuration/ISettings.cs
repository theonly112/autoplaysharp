using autoplaysharp.Contracts.Configuration.Tasks;

namespace autoplaysharp.Contracts.Configuration
{
    public interface ISettings
    {
        bool EnableOverlay { get; set; }
        bool RestartOnError { get; set; }
        string WindowName { get; set; }
        string[] RoutineItems { get; set; }
        string[] RoutineItemsState { get; set; }
        EmulatorType EmulatorType { get; set; }
        ITimelineBattleSettings TimelineBattle { get; }
        IAllianceBattleSettings AllianceBattle { get; }
        IDimensionMissionSettings DimensionMission { get; }
        ILegendaryBattleSettings LegendaryBattle { get; }
        IEpicQuestSettings EpicQuest { get; }
        IVideoCaptureSettings VideoCapture { get; }
    }
}
