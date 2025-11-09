// Runtime/Rest/Bootstrap/FlowcastRestSettings.cs
using System;
using System.Collections.Generic;
using Flowcast.Core.Environments;
using UnityEngine;

namespace Flowcast.Rest.Bootstrap
{
    [CreateAssetMenu(
        fileName = "FlowcastRestSettings",
        menuName = "Flowcast/Rest/Settings",
        order = 1)]
    public sealed class FlowcastRestSettings : ScriptableObject
    {
        [Header("Environments")]
        [Tooltip("Available environments (Dev, Test, Prod...).")]
        [SerializeField] private List<Environment> environments = new();

        [Tooltip("Environment used when no persisted or preferred selection is available.")]
        [SerializeField] private Environment defaultEnvironment;

        [Tooltip("Preferred environment used on first run before player selection.")]
        [SerializeField] private Environment preferredEnvironment;

        [Tooltip("Persist last selected environment across sessions (PlayerPrefs).")]
        [SerializeField] private bool persistSelection = true;

        [Tooltip("Optional PlayerPrefs key override for the active environment id.")]
        [SerializeField] private string prefsKey = EnvironmentProvider.DefaultPrefsKey;

        [Header("Auth (optional, OAuth2 refresh)")]
        public bool UseOAuth2;
        public string TokenEndpoint;
        public string ClientId;
        public string ClientSecret;
        public string InitialAccessToken;
        public string InitialRefreshToken;

        [Header("Serialization")]
        public bool PreferNewtonsoftIfDefined = false;

        [Header("Recording")]
        public bool EnableRecorder = false;
        public string RecordDirectory = "FlowcastRecords";

        public IReadOnlyList<Environment> Environments => environments;

        public Environment DefaultEnvironment =>
            defaultEnvironment != null ? defaultEnvironment :
            (environments.Count > 0 ? environments[0] : null);

        public Environment PreferredEnvironment => preferredEnvironment;

        public bool PersistSelection => persistSelection;

        public string PrefsKey => string.IsNullOrWhiteSpace(prefsKey)
            ? EnvironmentProvider.DefaultPrefsKey
            : prefsKey;

        public bool TryGetById(string id, out Environment env)
        {
            if (!string.IsNullOrEmpty(id))
            {
                foreach (var e in environments)
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

        public EnvironmentProvider.Configuration CreateConfiguration()
        {
            return new EnvironmentProvider.Configuration
            {
                Environments = environments,
                DefaultEnvironment = DefaultEnvironment,
                PreferredEnvironment = preferredEnvironment,
                PersistSelection = persistSelection,
                PrefsKey = prefsKey
            };
        }
    }
}
