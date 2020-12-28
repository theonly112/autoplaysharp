using System;
using System.IO;
using Microsoft.Extensions.Logging;

namespace autoplaysharp.App.Logging
{
    internal class CustomTraceProvider : ILoggerProvider
    {
        private readonly StreamWriter _streamWriter;

        public CustomTraceProvider()
        {
            Directory.CreateDirectory("logs");
            _streamWriter = new StreamWriter(@"logs\\trace.log", true);
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new TraceLogger(categoryName, _streamWriter);
        }

        public void Dispose()
        {
            _streamWriter.Dispose();
        }

        private class TraceLogger : ILogger
        {
            private readonly string _categoryName;
            private readonly StreamWriter _streamWriter;

            public TraceLogger(string categoryName, StreamWriter streamWriter)
            {
                _categoryName = categoryName.Contains(".") ? categoryName.Substring(categoryName.LastIndexOf('.') + 1) : categoryName;
                _streamWriter = streamWriter;
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
                _streamWriter.WriteLine(CustomConsoleProvider.CustomConsoleLogger.FormatMessage(_categoryName, logLevel, state, exception, formatter));
            }
        }
    }
}