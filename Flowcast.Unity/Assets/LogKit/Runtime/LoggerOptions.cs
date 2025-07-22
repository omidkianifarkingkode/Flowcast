using LogKit.Sinks;
using System.Collections.Generic;
using UnityEngine;

namespace LogKit
{
    [System.Serializable]
    public class LoggerOptions
    {
        public string Prefix = "Flowcast";
        public int MaxLength = 1024;
        public LogLevel MinimumLogLevel = LogLevel.Debug;
        [SerializeField] private Color _color = UnityEngine.Color.white;
        private string _colorString;

        public string Color
        {
            get
            {
                if (string.IsNullOrEmpty(_colorString))
                    _colorString = "#" + ColorUtility.ToHtmlStringRGB(_color);
                return _colorString;
            }
            set { _colorString = value; }
        }
        [TextArea(2, 5)]
        public string LogFormat = "[{prefix}][{level}] {message}";
        public bool IncludeTimestamp = false;
        public bool EnableUnitySink = true;
        public bool EnableFileSink = false;
        public int MaxLogFiles = 10;

        [System.Serializable]
        public class LogLevelColor
        {
            public LogLevel Level;
            public string Color;
        }

        public List<LogLevelColor> LevelColors = new()
        {
            new LogLevelColor { Level = LogLevel.Info, Color = "white" },
            new LogLevelColor { Level = LogLevel.Warning, Color = "yellow" },
            new LogLevelColor { Level = LogLevel.Error, Color = "red" },
            new LogLevelColor { Level = LogLevel.Debug, Color = "grey" },
            new LogLevelColor { Level = LogLevel.Exception, Color = "magenta" }
        };

        [System.NonSerialized] public List<ILogSink> Sinks = new();

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
