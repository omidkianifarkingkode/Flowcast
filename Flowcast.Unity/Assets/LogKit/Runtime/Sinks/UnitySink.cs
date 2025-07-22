using System.Linq;
using UnityEngine;

namespace LogKit.Sinks
{
    public class UnitySink : ILogSink
    {
        private readonly LoggerOptions _options;

        public UnitySink(LoggerOptions options)
        {
            _options = options;
        }

        public void Emit(LogEvent logEvent)
        {
            string message = FormatMessage(logEvent);
            switch (logEvent.Level)
            {
                case LogLevel.Info:
                    Debug.Log(message);
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning(message);
                    break;
                case LogLevel.Error:
                case LogLevel.Exception:
                    Debug.LogError(message);
                    break;
                case LogLevel.Debug:
                    Debug.Log(message);
                    break;
            }
        }

        private string FormatMessage(LogEvent logEvent)
        {
            string timestamp = _options.IncludeTimestamp ? $"[{logEvent.Timestamp:HH:mm:ss}] " : string.Empty;
            string message = logEvent.MessageTemplate ?? string.Empty;

            string levelColor = _options.GetLevelColorMap().TryGetValue(logEvent.Level, out var lc) ? lc : "white";
            string color = _options.Color;

            string properties = string.Empty;
            if (logEvent.Properties != null && logEvent.Properties.Count > 0)
            {
                properties = " | " + string.Join(", ", logEvent.Properties.Select(kv => $"{kv.Key}={kv.Value}"));
            }

            // Replace format placeholders
            string formatted = _options.LogFormat
                .Replace("{prefix}", $"<color={color}>{_options.Prefix}</color>")
                .Replace("{level}", $"<color={levelColor}>{logEvent.Level}</color>")
                .Replace("{message}", timestamp + message + properties);

            return formatted;

        }

    }
}
