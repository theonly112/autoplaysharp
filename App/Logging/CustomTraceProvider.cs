using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;

namespace autoplaysharp
{
    internal class CustomTraceProvider : ILoggerProvider
    {
        private List<ILogger> _loggers = new List<ILogger>();
        private StreamWriter _streamWriter;

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

        class TraceLogger : ILogger
        {
            private string _categoryName;
            private StreamWriter _streamWriter;

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
                _streamWriter.WriteLine(CustomConsoleProvider.CustomConsoleLogger.FormatMessage<TState>(_categoryName, logLevel, state, exception, formatter));
            }
        }
    }
}