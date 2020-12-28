using autoplaysharp.Contracts;
using autoplaysharp.Core.Game.UI;
using ImGuiNET;

namespace autoplaysharp.Overlay
{
    public class UiElementOverlayElement : OverlayElement
    {
        public readonly UiElement UiElement;
        protected readonly IEmulatorWindow EmulatorWindow;
        protected uint Color = 0xff00FFFF;

        public UiElementOverlayElement(UiElement uIElement, IEmulatorWindow emulatorWindow, int duration = 3000) : base(duration)
        {
            UiElement = uIElement;
            EmulatorWindow = emulatorWindow;
        }

        public override void Render(int delta)
        {
            base.Render(delta);
            var drawList = ImGui.GetBackgroundDrawList();
            drawList.AddRect(UiElement.GetDenormalizedLocation(EmulatorWindow), UiElement.GetDenormalizedLocationBottomRight(EmulatorWindow), Color);
            drawList.AddText(ImGui.GetFont(), 16, UiElement.GetDenormalizedLocation(EmulatorWindow), Color, UiElement.Id);
        }
    }
}
