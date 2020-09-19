using Microsoft.Extensions.Logging;
using System;

namespace autoplaysharp.App.Logging
{
    internal class CustomUILoggerProvider : ILoggerProvider, IUiLogger
    {
        public event Action<string> NewLogEntry;

        public ILogger CreateLogger(string categoryName)
        {
            return new UILogger(categoryName, this); ;
        }

        public void Dispose()
        {
        }

        private void OnLog(string v)
        {
            NewLogEntry?.Invoke(v);
        }

        private class UILogger : ILogger
        {
            private string _categoryName;
            private readonly CustomUILoggerProvider _customUILoggerProvider;

            public UILogger(string categoryName, CustomUILoggerProvider customUILoggerProvider)
            {
                _categoryName = categoryName.Contains(".") ? categoryName.Substring(categoryName.LastIndexOf('.') + 1) : categoryName;
                _customUILoggerProvider = customUILoggerProvider;
            }

            public IDisposable BeginScope<TState>(TState state)
            {
                return null;
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                _customUILoggerProvider.OnLog(CustomConsoleProvider.CustomConsoleLogger.FormatMessage<TState>(_categoryName, logLevel, state, exception, formatter));
            }
        }

    }
}
