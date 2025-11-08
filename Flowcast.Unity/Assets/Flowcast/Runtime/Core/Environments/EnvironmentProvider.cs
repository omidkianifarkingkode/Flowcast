// Runtime/Core/Environments/EnvironmentProvider.cs
using System;
using UnityEngine;

namespace Flowcast.Core.Environments
{
    /// <summary>
    /// Minimal runtime provider (no DI). Lazy-initializes from FlowcastClientSettings in Resources.
    /// </summary>
    public sealed class EnvironmentProvider : IEnvironmentProvider
    {
        public const string DefaultPrefsKey = "flowcast.active_env";

        private static EnvironmentProvider _instance;
        public static EnvironmentProvider Instance => _instance ??= new EnvironmentProvider();

        private Environment _current;
        private FlowcastClientSettings _settings;

        public event Action<Environment> Changed;

        private EnvironmentProvider() { }

        public Environment Current
        {
            get
            {
                EnsureInitialized();
                return _current;
            }
        }

        public string PersistedEnvironmentId
        {
            get
            {
                EnsureInitialized();
                return _settings.PersistSelection ? PlayerPrefs.GetString(_settings.PrefsKey, string.Empty) : string.Empty;
            }
        }

        public void Set(Environment env)
        {
            EnsureInitialized();
            if (env == null || _current == env) return;

            _current = env;

            if (_settings.PersistSelection)
            {
                PlayerPrefs.SetString(_settings.PrefsKey, env.Id);
                PlayerPrefs.Save();
            }

            Changed?.Invoke(_current);
            if (_settings != null && _settings.EnvironmentSet != null)
            {
                if (_current != null && _current.EnableLogging)
                    Debug.Log($"[Flowcast] Active environment set to '{_current.DisplayName}' ({_current.Id})");
            }
        }

        public void ClearPersistedSelection()
        {
            EnsureInitialized();
            if (_settings.PersistSelection)
            {
                PlayerPrefs.DeleteKey(_settings.PrefsKey);
            }
        }

        private void EnsureInitialized()
        {
            if (_settings != null) return;

            _settings = FlowcastClientSettings.LoadFromResources();
            if (_settings == null)
            {
                Debug.LogWarning("[Flowcast] ClientSettings not found in Resources/Flowcast/ClientSettings. Using empty defaults.");
                _current = null;
                return;
            }

            var set = _settings.EnvironmentSet;
            if (set == null)
            {
                Debug.LogWarning("[Flowcast] EnvironmentSet not assigned in ClientSettings.");
                _current = _settings.PreferredEnvironment;
                return;
            }

            // 1) Load persisted id if enabled
            if (_settings.PersistSelection)
            {
                var persistedId = PlayerPrefs.GetString(_settings.PrefsKey, string.Empty);
                if (!string.IsNullOrWhiteSpace(persistedId) && set.TryGetById(persistedId, out var persisted))
                {
                    _current = persisted;
                }
            }

            // 2) Preferred in settings
            if (_current == null && _settings.PreferredEnvironment != null)
                _current = _settings.PreferredEnvironment;

            // 3) Default in set
            if (_current == null)
                _current = set.DefaultEnvironmentFallback;

            if (_current != null && _current.EnableLogging)
                Debug.Log($"[Flowcast] Initial environment: '{_current.DisplayName}' ({_current.Id})");
        }
    }

    /// <summary>
    /// Small static helper for convenience access in user code.
    /// </summary>
    public static class FlowcastEnvironment
    {
        public static Environment Current => EnvironmentProvider.Instance.Current;

        public static void Set(Environment env) => EnvironmentProvider.Instance.Set(env);

        public static void ClearPersistedSelection() => EnvironmentProvider.Instance.ClearPersistedSelection();
    }
}
