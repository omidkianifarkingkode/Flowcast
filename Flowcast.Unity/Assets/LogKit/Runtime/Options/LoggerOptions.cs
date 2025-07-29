using LogKit.Sinks;
using System.Collections.Generic;

namespace LogKit
{
    public class LoggerOptions : ILoggerOptions 
    {
        public string Prefix { get; set; }
        public int MaxLength { get; set; } = 1024;
        public LogLevel MinimumLogLevel { get; set; } = LogLevel.Debug;
        public string Color { get; set; } = "white";
        public string LogFormat { get; set; } = "[{prefix}][{level}] {message}";
        public bool IncludeTimestamp { get; set; } = false;
        public bool EnableUnitySink { get; set; } = true;
        public bool EnableFileSink { get; set; } = false;
        public int MaxLogFiles { get; set; } = 10;

        public List<LogLevelColor> LevelColors { get; set; } = new()
        {
            new LogLevelColor { Level = LogLevel.Info, Color = "white" },
            new LogLevelColor { Level = LogLevel.Warning, Color = "yellow" },
            new LogLevelColor { Level = LogLevel.Error, Color = "red" },
            new LogLevelColor { Level = LogLevel.Debug, Color = "grey" },
            new LogLevelColor { Level = LogLevel.Exception, Color = "magenta" }
        };

        public List<ILogSink> Sinks { get; set; } = new();

        private Dictionary<LogLevel, string> _levelColorMap;

        public Dictionary<LogLevel, string> GetLevelColorMap()
        {
            if (_levelColorMap is not null)
                return _levelColorMap;

            _levelColorMap = new Dictionary<LogLevel, string>();
            foreach (var lc in LevelColors)
            {
                _levelColorMap[lc.Level] = lc.Color;
            }
            return _levelColorMap;
        }
    }
}
