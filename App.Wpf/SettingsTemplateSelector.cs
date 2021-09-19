using System;
using System.Windows;
using System.Windows.Controls;
using autoplaysharp.App.UI.Tasks;
using autoplaysharp.Core.Game.Tasks.Missions;
using autoplaysharp.Core.Game.Tasks.Missions.DualEpicQuests.Shifter;

namespace autoplaysharp.App
{
    public class SettingsTemplateSelector : DataTemplateSelector
    {
        public DataTemplate AllianceBattleTemplate { get; set; }
        public DataTemplate TimelineBattleTemplate { get; set; }
        public DataTemplate DimensionMissionTemplate { get; set; }
        public DataTemplate LegendaryBattleTemplate { get; set; }
        public DataTemplate PlaceHolderTemplate { get; set; }
        public DataTemplate EpicQuestTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            return item switch
            {
                RoutineViewModel vm => vm.TaskType switch
                {
                    _ when vm.TaskType == typeof(AllianceBattle) => AllianceBattleTemplate,
                    _ when vm.TaskType == typeof(TimelineBattle) => TimelineBattleTemplate,
                    _ when vm.TaskType == typeof(DimensionMission) => DimensionMissionTemplate,
                    _ when vm.TaskType == typeof(LegendaryBattle) => LegendaryBattleTemplate,
                    _ when vm.TaskType.IsAssignableTo(typeof(GenericEpicQuest)) => EpicQuestTemplate,
                    _ => PlaceHolderTemplate
                },
                _ => null
            };
        }
    }
}