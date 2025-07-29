using UnityEngine;

namespace LogKit.Bootstrap
{
    public static class LoggerBootstrapper
    {
        private static bool _initialized = false;

        public static ILoggerOptions DefaultOptions { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            if (_initialized) return;

            DefaultOptions = LoggerOptionsAsset.LoadDefault();
            if (DefaultOptions == null)
            {
                Debug.LogWarning("[LoggerBootstrapper] LoggerOptionsAsset asset not found in Resources. Using fallback defaults.");
                DefaultOptions = GetFallbackOptions(); // fallback will be used below
            }

            new LoggerFactory(o =>
            {
                o.EnableUnitySink = DefaultOptions.EnableUnitySink;
                o.EnableFileSink = DefaultOptions.EnableFileSink;
                o.MinimumLogLevel = DefaultOptions.MinimumLogLevel;
                o.IncludeTimestamp = DefaultOptions.IncludeTimestamp;
                o.LogFormat = DefaultOptions.LogFormat;
                o.MaxLength = DefaultOptions.MaxLength;
                o.Prefix = DefaultOptions.Prefix;
                o.Color = DefaultOptions.Color;
                o.MaxLogFiles= DefaultOptions.MaxLogFiles;

                o.LevelColors.Clear();
                o.LevelColors.AddRange(DefaultOptions.LevelColors);
            });

            _initialized = true;
        }

        private static ILoggerOptions GetFallbackOptions()
        {
            return new LoggerOptions
            {
                EnableUnitySink = true,
                EnableFileSink = false,
                MinimumLogLevel = LogLevel.Debug,
                IncludeTimestamp = true,
                LogFormat = "[{prefix}][{level}] {message}",
                Prefix = "Static",
                Color = "#00FFAA"
            };
        }
    }
}
