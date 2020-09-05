using autoplaysharp.Game.UI;

namespace autoplaysharp.Contracts
{
    public interface IEmulatorOverlay
    {
        void ShowGetText(UIElement uIElement);
        void ShowIsVisibile(UIElement uIElement, bool isVisible);
    }
}
