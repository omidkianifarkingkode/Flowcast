using LogKit.Sinks;
using System.Collections.Generic;
using UnityEngine;

namespace LogKit
{
    public interface ILoggerOptions
    {
        string Prefix { get; set; }
        int MaxLength { get; set; }
        LogLevel MinimumLogLevel { get; set; }
        string Color { get; set; }
        string LogFormat { get; set; }
        bool IncludeTimestamp { get; set; }
        bool EnableUnitySink { get; set; }
        bool EnableFileSink { get; set; }
        int MaxLogFiles { get; set; }

        List<LogLevelColor> LevelColors { get; set; }

        List<ILogSink> Sinks { get; set; }

        Dictionary<LogLevel, string> GetLevelColorMap();
    }
}
