using Microsoft.Extensions.Logging;
using System;

namespace autoplaysharp.App.Logging
{
    internal class CustomUiLoggerProvider : ILoggerProvider, IUiLogger
    {
        public event Action<string> NewLogEntry;

        public ILogger CreateLogger(string categoryName)
        {
            return new UiLogger(categoryName, this);
        }

        public void Dispose()
        {
        }

        private void OnLog(string v)
        {
            NewLogEntry?.Invoke(v);
        }

        private class UiLogger : ILogger
        {
            private readonly string _categoryName;
            private readonly CustomUiLoggerProvider _customUiLoggerProvider;

            public UiLogger(string categoryName, CustomUiLoggerProvider customUiLoggerProvider)
            {
                _categoryName = categoryName.Contains(".") ? categoryName.Substring(categoryName.LastIndexOf('.') + 1) : categoryName;
                _customUiLoggerProvider = customUiLoggerProvider;
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
                _customUiLoggerProvider.OnLog(CustomConsoleProvider.CustomConsoleLogger.FormatMessage(_categoryName, logLevel, state, exception, formatter));
            }
        }

    }
}
