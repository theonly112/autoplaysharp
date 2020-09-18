using autoplaysharp.Contracts;
using autoplaysharp.Core.Game.UI;
using autoplaysharp.Game.UI;
using ImGuiNET;

namespace autoplaysharp.Overlay
{
    public class UiElementOverlayElement : OverlayElement
    {
        public readonly UIElement UIElement;
        protected readonly IEmulatorWindow EmulatorWindow;
        protected uint Color = 0xff00FFFF;

        public UiElementOverlayElement(UIElement uIElement, IEmulatorWindow emulatorWindow, int duration = 3000) : base(duration)
        {
            UIElement = uIElement;
            EmulatorWindow = emulatorWindow;
        }

        public override void Render(int delta)
        {
            base.Render(delta);
            var drawList = ImGui.GetBackgroundDrawList();
            drawList.AddRect(UIElement.GetDenormalizedLocation(EmulatorWindow), UIElement.GetDenormalizedLocationBottomRight(EmulatorWindow), Color);
            drawList.AddText(ImGui.GetFont(), 16, UIElement.GetDenormalizedLocation(EmulatorWindow), Color, UIElement.Id);
        }
    }
}
