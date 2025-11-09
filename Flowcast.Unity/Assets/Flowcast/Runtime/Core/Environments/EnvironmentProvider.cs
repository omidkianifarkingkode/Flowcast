// Runtime/Core/Environments/EnvironmentProvider.cs
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Flowcast.Core.Environments
{
    /// <summary>
    /// Minimal runtime provider (no DI). Configure once at startup to populate environments.
    /// </summary>
    public sealed class EnvironmentProvider : IEnvironmentProvider
    {
        public const string DefaultPrefsKey = "flowcast.active_env";

        public struct Configuration
        {
            public IReadOnlyList<Environment> Environments;
            public Func<Environment> ResolveInitialEnvironment;
            public bool PersistSelection;
            public string PrefsKey;
        }

        private static EnvironmentProvider _instance;
        public static EnvironmentProvider Instance => _instance ??= new EnvironmentProvider();

        private Environment _current;
        private IReadOnlyList<Environment> _environments = Array.Empty<Environment>();
        private Func<Environment> _resolveInitialEnvironment;
        private bool _persistSelection = true;
        private string _prefsKey = DefaultPrefsKey;
        private bool _configured;
        private bool _warnedUnconfigured;

        public event Action<Environment> Changed;

        private EnvironmentProvider() { }

        public Environment Current
        {
            get
            {
                EnsureConfigured();
                return _current;
            }
        }

        public string PersistedEnvironmentId
        {
            get
            {
                EnsureConfigured();
                if (!_configured || !_persistSelection) return string.Empty;
                return PlayerPrefs.GetString(_prefsKey, string.Empty);
            }
        }

        public void Configure(Configuration configuration)
        {
            _environments = configuration.Environments ?? Array.Empty<Environment>();
            _resolveInitialEnvironment = configuration.ResolveInitialEnvironment ?? (() =>
            {
                foreach (var env in _environments)
                {
                    if (env != null)
                        return env;
                }

                return null;
            });
            _persistSelection = configuration.PersistSelection;
            _prefsKey = string.IsNullOrWhiteSpace(configuration.PrefsKey) ? DefaultPrefsKey : configuration.PrefsKey;

            _configured = true;
            _warnedUnconfigured = false;

            InitializeCurrentFromConfiguration();
        }

        public void Set(Environment env)
        {
            EnsureConfigured();
            if (!_configured || env == null || _current == env) return;

            _current = env;

            if (_persistSelection)
            {
                PlayerPrefs.SetString(_prefsKey, env.Id);
                PlayerPrefs.Save();
            }

            Changed?.Invoke(_current);

            if (_current != null && _current.EnableLogging)
                Debug.Log($"[Flowcast] Active environment set to '{_current.DisplayName}' ({_current.Id})");
        }

        public void ClearPersistedSelection()
        {
            EnsureConfigured();
            if (!_configured || !_persistSelection) return;

            PlayerPrefs.DeleteKey(_prefsKey);
        }

        private void InitializeCurrentFromConfiguration()
        {
            _current = null;

            if (_persistSelection)
            {
                var persistedId = PlayerPrefs.GetString(_prefsKey, string.Empty);
                if (!string.IsNullOrWhiteSpace(persistedId) && TryFindById(persistedId, out var persisted))
                {
                    _current = persisted;
                }
            }

            if (_current == null && _resolveInitialEnvironment != null)
            {
                try
                {
                    _current = _resolveInitialEnvironment();
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[Flowcast] Failed to resolve initial environment: {ex.Message}\n{ex}");
                }
            }

            if (_current == null)
            {
                foreach (var env in _environments)
                {
                    if (env != null)
                    {
                        _current = env;
                        break;
                    }
                }
            }

            if (_current != null && _current.EnableLogging)
                Debug.Log($"[Flowcast] Initial environment: '{_current.DisplayName}' ({_current.Id})");
        }

        private bool TryFindById(string id, out Environment env)
        {
            if (!string.IsNullOrEmpty(id))
            {
                foreach (var e in _environments)
                {
                    if (e != null && string.Equals(e.Id, id, StringComparison.OrdinalIgnoreCase))
                    {
                        env = e;
                        return true;
                    }
                }
            }

            env = null;
            return false;
        }

        private void EnsureConfigured()
        {
            if (_configured) return;

            if (_warnedUnconfigured) return;
            _warnedUnconfigured = true;
            Debug.LogWarning("[Flowcast] EnvironmentProvider has not been configured. Assign FlowcastRestSettings on FlowcastRestBootstrapper.");
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

        public static void Configure(EnvironmentProvider.Configuration configuration)
            => EnvironmentProvider.Instance.Configure(configuration);
    }
}
