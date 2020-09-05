using autoplaysharp.Game.UI;
using Microsoft.Extensions.Logging;
using System;

namespace autoplaysharp.Contracts
{
    public interface IGame
    {
        void Click(string id);
        string GetText(string id);
        bool IsVisible(string id);
        void Click(UIElement element);
        string GetText(UIElement element);
        bool IsVisible(UIElement element);
        void Drag(string idStart, string IdEnd);

        // TODO: find a better solution to create loggers...
        ILogger CreateLogger(Type t);
    }
}
