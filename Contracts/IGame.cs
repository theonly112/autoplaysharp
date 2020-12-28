using autoplaysharp.Contracts.Errors;
using Microsoft.Extensions.Logging;
using System;

namespace autoplaysharp.Contracts
{
    public interface IGame
    {
        void Click(string id);
        string GetText(string id);
        bool IsVisible(string id);
        void Click(UiElement element);
        string GetText(UiElement element);
        bool IsVisible(UiElement element);
        void Drag(string idStart, string idEnd);

        // TODO: find a better solution to create loggers...
        ILogger CreateLogger(Type t);

        IEmulatorOverlay Overlay { get; set; }

        void OnError(TaskError error);
    }
}
