using autoplaysharp.Contracts;
using autoplaysharp.Core.Game.UI;
using ImGuiNET;
using System.Numerics;

namespace autoplaysharp.Overlay
{
    public class IsVisibleUiElement : UiElementOverlayElement
    {
        private readonly double? _certainty;

        public IsVisibleUiElement(UIElement element, IEmulatorWindow window, bool isVisible, double? certainty = null) : base(element, window, isVisible ? 5000 : 3000)
        {
            Color = isVisible ? 0xff00ff00 : 0xff0000ff; // Green for found, red for not found.
            _certainty = certainty;
        }

        public override void Render(int delta)
        {
            base.Render(delta);
            var drawList = ImGui.GetBackgroundDrawList();
            var certainty = $"{_certainty * 100f:0}%";
            drawList.AddText(ImGui.GetFont(), 16, UIElement.GetDenormalizedLocation(EmulatorWindow) + new Vector2(0, 16), Color, certainty);
        }
    }
}
