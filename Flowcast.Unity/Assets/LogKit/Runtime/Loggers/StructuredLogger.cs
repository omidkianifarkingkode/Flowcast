using LogKit.Sinks;
using System;
using System.Collections.Generic;

namespace LogKit.Loggers
{
    public class StructuredLogger : ILogger
    {
        private readonly ILoggerOptions _options;

        public StructuredLogger(ILoggerOptions options)
        {
            _options = options;
        }

        public bool IsEnabled { get; set; } = true;
        public LogLevel MinimumLogLevel => _options.MinimumLogLevel;

        public void Log(string message, LogLevel level = LogLevel.Info)
        {
            if (!IsEnabled || level < MinimumLogLevel) return;
            Emit(new LogEvent { MessageTemplate = message, Level = level });
        }

        public void Log(Exception exception, string message = null, LogLevel level = LogLevel.Exception)
        {
            if (!IsEnabled || level < MinimumLogLevel) return;

            Emit(new LogEvent
            {
                Level = level,
                MessageTemplate = message != null ? $"{message}\n{exception}" : exception.ToString(),
                Exception = exception
            });
        }

        public void Log(string message, LogLevel level, Dictionary<string, object> properties)
        {
            if (!IsEnabled || level < MinimumLogLevel) return;

            Emit(new LogEvent
            {
                MessageTemplate = message,
                Level = level,
                Properties = properties ?? new Dictionary<string, object>()
            });
        }


        private void Emit(LogEvent logEvent)
        {
            foreach (var sink in _options.Sinks)
            {
                sink.Emit(logEvent);
            }
        }

        public void LogInfo(string message) => Log(message, LogLevel.Info);
        public void LogWarning(string message) => Log(message, LogLevel.Warning);
        public void LogError(string message) => Log(message, LogLevel.Error);
        public void LogDebug(string message) => Log(message, LogLevel.Debug);
        public void LogException(Exception exception) => Log(exception);
        public void LogException(Exception exception, string message) => Log(exception, message);
        public void LogException(Exception exception, string message, LogLevel level) => Log(exception, message, level);
        public void LogInfo(string message, Dictionary<string, object> properties) => Log(message, LogLevel.Info, properties);

    }
}
