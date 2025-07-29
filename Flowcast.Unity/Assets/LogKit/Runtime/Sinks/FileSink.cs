using System;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace LogKit.Sinks
{
    public class FileSink : ILogSink
    {
        private readonly string _filePath;
        private readonly ILoggerOptions _options;

        public FileSink(ILoggerOptions options)
        {
            _options = options;

#if UNITY_EDITOR
            string directory = Path.Combine(Application.dataPath, "../Logs");
#else
    string directory = Path.Combine(Application.persistentDataPath, "Logs");
#endif


            Directory.CreateDirectory(directory);
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            _filePath = Path.Combine(directory, $"log_{timestamp}.log");
        }

        public void Emit(LogEvent logEvent)
        {
            var sb = new StringBuilder();
            sb.Append('[')
                .Append(logEvent.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"))
                .Append("] [")
                .Append(logEvent.Level)
                .Append("] ")
                .Append(logEvent.MessageTemplate);

            if (logEvent.Properties != null && logEvent.Properties.Count > 0)
            {
                var props = string.Join(", ", logEvent.Properties.Select(kv => $"{kv.Key}={kv.Value}"));
                sb.Append(" | ").Append(props);
            }

            if (logEvent.Exception != null)
            {
                sb.Append('\n').Append(logEvent.Exception);
            }

            try
            {
                File.AppendAllText(_filePath, sb.ToString() + Environment.NewLine);
            }
            catch (IOException ex)
            {
                Debug.LogError($"[FileSink] Failed to write log: {ex}");
            }
        }

    }
}
