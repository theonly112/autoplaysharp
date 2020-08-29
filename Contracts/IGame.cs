using autoplaysharp.Game.UI;

namespace autoplaysharp.Contracts
{
    interface IGame
    {
        void Click(string id);
        string GetText(string id);
        bool IsVisible(string id);
        void Click(UIElement element);
        string GetText(UIElement element);
        bool IsVisible(UIElement element);
        void Drag(string idStart, string IdEnd);
    }
}
