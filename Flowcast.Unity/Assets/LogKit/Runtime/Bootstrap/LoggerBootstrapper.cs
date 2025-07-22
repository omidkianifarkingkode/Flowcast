using UnityEngine;

namespace LogKit.Bootstrap
{
    public static class LoggerBootstrapper
    {
        private static bool _initialized = false;

        public static LoggerOptions DefaultOptions { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Initialize()
        {
            if (_initialized) return;

            var config = Resources.Load<LoggerConfig>("LoggerConfig");
            if (config == null)
            {
                Debug.LogWarning("[LoggerBootstrapper] LoggerConfig asset not found in Resources. Using fallback defaults.");
                DefaultOptions = GetFallbackOptions(); // fallback will be used below
            }
            else
            {
                DefaultOptions = config.Options;
            }

            LoggerOptions options = config != null
                ? config.Options
                : GetFallbackOptions();

            new LoggerFactory(o =>
            {
                o.EnableUnitySink = options.EnableUnitySink;
                o.EnableFileSink = options.EnableFileSink;
                o.MinimumLogLevel = options.MinimumLogLevel;
                o.IncludeTimestamp = options.IncludeTimestamp;
                o.LogFormat = options.LogFormat;
                o.MaxLength = options.MaxLength;
                o.Prefix = options.Prefix;
                o.Color = options.Color;
                o.MaxLogFiles= options.MaxLogFiles;

                o.LevelColors.Clear();
                o.LevelColors.AddRange(options.LevelColors);
            });

            _initialized = true;

#if UNITY_EDITOR
            if (Application.isPlaying)
                EnsureEditorWidgetExists();
#endif
        }

        private static LoggerOptions GetFallbackOptions()
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

#if UNITY_EDITOR
        private static void EnsureEditorWidgetExists()
        {
            var obj = Object.FindObjectOfType<EditorLoggerWidget>();
            if (obj != null) return;

            var go = new GameObject("LoggerWidget");
            go.AddComponent<LoggerWidget>();
            Object.DontDestroyOnLoad(go);
        }
#endif
    }
}
