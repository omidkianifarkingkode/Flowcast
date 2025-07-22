using LogKit.Loggers;
using LogKit.Sinks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LogKit
{
    public class LoggerFactory
    {
        private readonly Dictionary<string, ILogger> _loggers = new();
        private LoggerOptions _defaultOptions;
        private FileSink _sharedFileSink;

        private readonly object _lock = new();

        private static LoggerFactory _instance;

        public static ILogger Create(string module, Action<LoggerOptions> overrideOptions = null)
        {
            if (_instance == null)
            {
                Debug.LogWarning($"[LoggerFactory] Instance not initialized. Did you forget to add a LoggerBootstrapper to the scene?");
                return null;
            }

            return _instance.CreateLogger(module, overrideOptions);
        }


        public LoggerFactory(Action<LoggerOptions> configure)
        {
            _defaultOptions = new LoggerOptions();
            configure?.Invoke(_defaultOptions);

            if (_defaultOptions.EnableUnitySink)
                _defaultOptions.Sinks.Add(new UnitySink(_defaultOptions));
            if (_defaultOptions.EnableFileSink)
            {
                _sharedFileSink = new FileSink(_defaultOptions); // prune once here
                _defaultOptions.Sinks.Add(_sharedFileSink);
            }

            _instance = this;
        }

        public ILogger CreateLogger(string module, Action<LoggerOptions> overrideOptions = null)
        {
            lock (_lock)
            {
                if (_loggers.TryGetValue(module, out var existing))
                    return existing;

                var options = CloneOptions(_defaultOptions);
                options.Prefix = module;
                overrideOptions?.Invoke(options);

                options.Sinks.Clear();
                if (options.EnableUnitySink)
                    options.Sinks.Add(new UnitySink(options));
                if (options.EnableFileSink)
                    options.Sinks.Add(_sharedFileSink);

                var logger = new StructuredLogger(options);
                _loggers[module] = logger;
                return logger;
            }
        }


        private LoggerOptions CloneOptions(LoggerOptions source)
        {
            var clone = new LoggerOptions
            {
                Prefix = source.Prefix,
                MaxLength = source.MaxLength,
                MinimumLogLevel = source.MinimumLogLevel,
                Color = source.Color,
                LogFormat = source.LogFormat,
                IncludeTimestamp = source.IncludeTimestamp,
                EnableUnitySink = source.EnableUnitySink,
                EnableFileSink = source.EnableFileSink,
                MaxLogFiles= source.MaxLogFiles,
                LevelColors = new List<LoggerOptions.LogLevelColor>()
            };

            foreach (var lc in source.LevelColors)
            {
                clone.LevelColors.Add(new LoggerOptions.LogLevelColor
                {
                    Level = lc.Level,
                    Color = lc.Color
                });
            }

            return clone;
        }

    }
}
