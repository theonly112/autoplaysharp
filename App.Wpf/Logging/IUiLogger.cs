using System;

namespace autoplaysharp.App.Logging
{
    internal interface IUiLogger
    {
        event Action<string> NewLogEntry;
    }
}
