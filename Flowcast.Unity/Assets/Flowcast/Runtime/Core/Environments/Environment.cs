// Runtime/Core/Environments/Environment.cs  (only the REST options part shown)
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Flowcast.Core.Environments
{
    [Serializable]
    public sealed class Environment : ScriptableObject
    {
        [Header("Basics")]
        public string Id = "dev";
        public string DisplayName = "Development";
        public string BaseUrl = "https://api.example.com";

        [Header("Defaults")]
        public int TimeoutSeconds = 15;
        public bool EnableLogging = true;

        [Header("Default Headers")]
        public List<NameValue> DefaultHeaders = new();

        // NEW: REST-agnostic options (no reference to Flowcast.Rest types)
        [Header("REST (Module-Agnostic)")]
        public RestOptionsCore Rest = new RestOptionsCore();

        [Serializable]
        public struct NameValue { public string Name; public string Value; }

        public Dictionary<string, string> GetDefaultHeadersDictionary()
        {
            var d = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var h in DefaultHeaders)
            {
                if (!string.IsNullOrWhiteSpace(h.Name))
                    d[h.Name] = h.Value ?? string.Empty;
            }
            return d;
        }
    }

    /// <summary>
    /// Module-agnostic REST settings stored in Core, later interpreted by Flowcast.Rest bootstrapper.
    /// </summary>
    [Serializable]
    public sealed class RestOptionsCore
    {
        [Header("Feature Defaults")]
        public bool DefaultLogging = true;
        public bool DefaultRetry = true;
        public bool DefaultRateLimit = true;
        public bool DefaultCircuitBreaker = true;
        public bool DefaultCaching = false;   // opt-in
        public bool DefaultAuth = false;      // opt-in

        [Header("Cache")]
        public bool CacheSWR = true;
        public int CacheDefaultTtlSeconds = 60;

        [Header("Retry")]
        public int RetryMaxAttempts = 3;
        public int RetryBaseDelayMs = 250;

        [Header("Rate Limiter")]
        public int RateLimitCapacity = 8;
        public double RateLimitRefillPerSecond = 4;

        [Header("Circuit Breaker")]
        public int BreakerFailureThreshold = 5;
        public int BreakerOpenMs = 5000;

        [Header("Transport")]
        public RestTransportModeCore TransportMode = RestTransportModeCore.Real;
        public string ReplayDirectory = "FlowcastRecords";
        public int MockDefaultLatencyMs = 0;

        [Header("Prefix Policies")]
        public List<PrefixPolicyCore> PrefixPolicies = new();
    }

    [Serializable]
    public struct PrefixPolicyCore
    {
        public string PathPrefix;        // e.g. "/v1/catalog"
        public bool RequireAuth;
        public bool EnableCache;
        public int CacheTtlSeconds;      // 0 = env default
        public bool DisableRetry;
        public string RateLimitKey;
    }

    public enum RestTransportModeCore { Real, Mock, Replay }
}
