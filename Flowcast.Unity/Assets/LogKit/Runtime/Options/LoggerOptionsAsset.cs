using LogKit.Sinks;
using System.Collections.Generic;
using UnityEngine;

namespace LogKit
{
    public partial class LoggerOptionsAsset : ScriptableObject, ILoggerOptions
    {
        public const string FileName = "LoggerOptions";
        public const string ResourceLoadPath = "Logkit/" + FileName;

        private static LoggerOptionsAsset _instance;

        public static LoggerOptionsAsset LoadDefault()
        {
            _instance ??= Resources.Load<LoggerOptionsAsset>(ResourceLoadPath);

            if (_instance == null)
                throw new MissingReferenceException($"LoggerOptionsAsset could not be found at Resources/{ResourceLoadPath}") ;

            return _instance;
        }

        [field: SerializeField] public string Prefix { get; set; }
        [field: SerializeField] public int MaxLength { get; set; } = 1024;
        [field: SerializeField] public LogLevel MinimumLogLevel { get; set; } = LogLevel.Debug;
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
        [SerializeField] private Color _color = UnityEngine.Color.white;
        private string _colorString; // cached string

        [field: SerializeField] public string LogFormat { get; set; } = "[{prefix}][{level}] {message}";
        [field: SerializeField] public bool IncludeTimestamp { get; set; } = false;
        [field: SerializeField] public bool EnableUnitySink { get; set; } = true;
        [field: SerializeField] public bool EnableFileSink { get; set; } = false;
        [field: SerializeField] public int MaxLogFiles { get; set; } = 10;

        [field: SerializeField]
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
