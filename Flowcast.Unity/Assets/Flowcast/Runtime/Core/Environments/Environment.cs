// Runtime/Core/Environments/Environment.cs
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Flowcast.Core.Environments
{
    [CreateAssetMenu(
        fileName = "Environment",
        menuName = "Flowcast/Core/Environment",
        order = 0)]
    public sealed class Environment : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string id = "dev";
        [SerializeField] private string displayName = "Development";

        [Header("HTTP")]
        [SerializeField] private string baseUrl = "https://api.example.dev";
        [SerializeField] private int timeoutSeconds = 30;
        [SerializeField] private bool enableLogging = true;

        [Header("Default Headers (optional)")]
        [SerializeField] private List<Header> defaultHeaders = new();

        [TextArea(2, 6)]
        [SerializeField] private string description;

        [Serializable]
        public struct Header
        {
            public string Name;
            public string Value;
        }

        public string Id => id;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? id : displayName;
        public string BaseUrl => baseUrl;
        public int TimeoutSeconds => Mathf.Max(1, timeoutSeconds);
        public bool EnableLogging => enableLogging;
        public IReadOnlyList<Header> DefaultHeaders => defaultHeaders;
        public string Description => description;

        public IReadOnlyDictionary<string, string> GetDefaultHeadersDictionary()
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var h in defaultHeaders)
            {
                if (string.IsNullOrEmpty(h.Name)) continue;
                dict[h.Name] = h.Value ?? string.Empty;
            }
            return dict;
        }
    }
}
