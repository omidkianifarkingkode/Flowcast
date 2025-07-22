using System;
using System.Collections.Generic;

namespace LogKit
{
    public interface ILogger
    {
        bool IsEnabled { get; }
        LogLevel MinimumLogLevel { get; }

        void Log(string message, LogLevel level = LogLevel.Info);
        void Log(Exception exception, string message = null, LogLevel level = LogLevel.Exception);
        void Log(string message, LogLevel level, Dictionary<string, object> properties);
        void LogInfo(string message, Dictionary<string, object> properties);

        void LogInfo(string message);
        void LogWarning(string message);
        void LogError(string message);
        void LogDebug(string message);

        void LogException(Exception exception);
        void LogException(Exception exception, string message);
        void LogException(Exception exception, string message, LogLevel level);
    }
}
