using System.Numerics;

namespace autoplaysharp.Contracts
{
    public interface IEmulatorOverlay
    {
        void ShowGetText(UIElement uIElement);
        void ShowIsVisibile(UIElement uIElement, bool isVisible);
        void ShowIsVisibile(UIElement uIElement, bool isVisible, double certainty);
        UIElement SelectedUiElement { get; set; }
        (Vector2 Position, Vector2 Size) SelectionBox { get; set; }
        bool PreviewElementText { get; set; }
    }
}
