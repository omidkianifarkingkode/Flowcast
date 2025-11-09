// Runtime/Rest/Bootstrap/FlowcastRestSettings.cs
using System;
using System.Collections.Generic;
using Flowcast.Core.Environments;
using Flowcast.Rest.Workbench;
using UnityEngine;
using Environment = Flowcast.Core.Environments.Environment;

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

        [Header("Active Environment")]
        [Tooltip("Environment used while running in the Unity Editor.")]
        [SerializeField] private Environment editorEnvironment;

        [Tooltip("Environment used when the application runs as a development build.")]
        [SerializeField] private Environment developmentEnvironment;

        [Tooltip("Environment used when the application runs as a non-development build.")]
        [SerializeField] private Environment releaseEnvironment;

        [Tooltip("Persist last selected environment across sessions (PlayerPrefs).")]
        [SerializeField] private bool persistSelection = true;

        [Tooltip("Optional PlayerPrefs key override for the active environment id.")]
        [SerializeField] private string prefsKey = EnvironmentProvider.DefaultPrefsKey;

        [Header("Workbench")]
        [SerializeField] private List<RequestAsset> requestAssets = new();

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

        public Environment EditorEnvironment => ResolveOrFallback(editorEnvironment);

        public Environment DevelopmentEnvironment => ResolveOrFallback(developmentEnvironment, releaseEnvironment);

        public Environment ReleaseEnvironment => ResolveOrFallback(releaseEnvironment, developmentEnvironment);

        public Environment ActiveEnvironment
        {
            get
            {
                if (Application.isEditor)
                {
                    return ResolveOrFallback(editorEnvironment, developmentEnvironment, releaseEnvironment);
                }

                if (Debug.isDebugBuild)
                {
                    return ResolveOrFallback(developmentEnvironment, releaseEnvironment, editorEnvironment);
                }

                return ResolveOrFallback(releaseEnvironment, developmentEnvironment, editorEnvironment);
            }
        }

        public bool PersistSelection => persistSelection;

        public string PrefsKey => string.IsNullOrWhiteSpace(prefsKey)
            ? EnvironmentProvider.DefaultPrefsKey
            : prefsKey;

        public IReadOnlyList<RequestAsset> RequestAssets => requestAssets;

        public bool TryGetRequestAsset(string nameOrId, out RequestAsset asset)
        {
            if (!string.IsNullOrEmpty(nameOrId))
            {
                foreach (var request in requestAssets)
                {
                    if (request == null)
                    {
                        continue;
                    }

                    if (!string.IsNullOrEmpty(request.RequestId) &&
                        string.Equals(request.RequestId, nameOrId, StringComparison.OrdinalIgnoreCase))
                    {
                        asset = request;
                        return true;
                    }

                    if (!string.IsNullOrEmpty(request.DisplayName) &&
                        string.Equals(request.DisplayName, nameOrId, StringComparison.OrdinalIgnoreCase))
                    {
                        asset = request;
                        return true;
                    }

                    if (string.Equals(request.name, nameOrId, StringComparison.OrdinalIgnoreCase))
                    {
                        asset = request;
                        return true;
                    }
                }
            }

            asset = null;
            return false;
        }

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
                ResolveInitialEnvironment = GetActiveEnvironment,
                PersistSelection = persistSelection,
                PrefsKey = prefsKey
            };
        }

        public Environment GetActiveEnvironment() => ActiveEnvironment;

        private Environment ResolveOrFallback(params Environment[] candidates)
        {
            if (candidates != null)
            {
                foreach (var candidate in candidates)
                {
                    if (candidate != null)
                    {
                        return candidate;
                    }
                }
            }

            foreach (var environment in environments)
            {
                if (environment != null)
                {
                    return environment;
                }
            }

            return null;
        }
    }
}
