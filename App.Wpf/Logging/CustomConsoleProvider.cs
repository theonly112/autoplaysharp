using System;
using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace autoplaysharp
{
    class CustomConsoleProvider : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            return new CustomConsoleLogger(categoryName);
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }

        internal class CustomConsoleLogger : ILogger
        {
            private string _categoryName;

            public CustomConsoleLogger(string categoryName)
            {
                _categoryName = categoryName.Contains(".") ? categoryName.Substring(categoryName.LastIndexOf('.') + 1) : categoryName;
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
                Trace.WriteLine(FormatMessage(_categoryName, logLevel, state, exception, formatter));
            }

            internal static string FormatMessage<TState>(string categoryName, LogLevel logLevel, TState state, Exception exception, Func<TState, Exception, string> formatter)
            {
                if (categoryName != null)
                {
                    return $"{DateTime.Now:HH:mm:ss.fff} {logLevel} - {categoryName} - {formatter(state, exception)}";
                }
                else
                {
                    return $"{DateTime.Now:HH:mm:ss.fff} {logLevel} - {formatter(state, exception)}";
                }
            }
        }
    }
}
