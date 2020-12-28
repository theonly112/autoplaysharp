using System.Numerics;

namespace autoplaysharp.Contracts
{
    public interface IEmulatorOverlay
    {
        void ShowGetText(UiElement uIElement);
        void ShowIsVisibile(UiElement uIElement, bool isVisible);
        void ShowIsVisibile(UiElement uIElement, bool isVisible, double certainty);
        UiElement SelectedUiElement { get; set; }
        (Vector2 Position, Vector2 Size) SelectionBox { get; set; }
        bool PreviewElementText { get; set; }
    }
}
