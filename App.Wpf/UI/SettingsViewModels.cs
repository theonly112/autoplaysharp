using autoplaysharp.App.UI.Tasks.AllianceBattle;
using autoplaysharp.App.UI.Tasks.DimensionMissions;
using autoplaysharp.App.UI.Tasks.EpicQuest;
using autoplaysharp.App.UI.Tasks.LegendaryBattle;
using autoplaysharp.App.UI.Tasks.TimelineBattle;
using Microsoft.Extensions.DependencyInjection;

namespace autoplaysharp.App.UI
{
    internal class SettingsViewModels
    {
        public AllianceBattleSettingsViewModel AllianceBattle =>
            App.ServiceProvider.GetService<AllianceBattleSettingsViewModel>();

        public TimelineBattleSettingsViewModel TimelineBattle =>
            App.ServiceProvider.GetService<TimelineBattleSettingsViewModel>();

        public DimensionMissionSettingsViewModel DimensionMissions =>
            App.ServiceProvider.GetService<DimensionMissionSettingsViewModel>();

        public EpicQuestSettingsViewModel EpicQuest =>
            App.ServiceProvider.GetService<EpicQuestSettingsViewModel>();

        public LegendaryBattleSettingsViewModel LegendaryBattle =>
            App.ServiceProvider.GetService<LegendaryBattleSettingsViewModel>();

        public static void ConfigureServices(ServiceCollection serviceCollection)
        {
            serviceCollection.AddSingleton<AllianceBattleSettingsViewModel>();
            serviceCollection.AddSingleton<TimelineBattleSettingsViewModel>();
            serviceCollection.AddSingleton<DimensionMissionSettingsViewModel>();
            serviceCollection.AddSingleton<EpicQuestSettingsViewModel>();
            serviceCollection.AddSingleton<LegendaryBattleSettingsViewModel>();
        }
    }
}
