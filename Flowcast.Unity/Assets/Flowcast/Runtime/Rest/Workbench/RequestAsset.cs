// Runtime/Rest/Workbench/RequestAsset.cs
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Flowcast.Rest.Workbench
{
    /// A saved REST request that the Workbench can run and re-run.
    [CreateAssetMenu(fileName = "RequestAsset", menuName = "Flowcast/Rest/Request Asset", order = 10)]
    public sealed class RequestAsset : ScriptableObject
    {
        public enum MethodKind { GET, POST, PUT, PATCH, DELETE, HEAD }

        [Header("Identity")]
        [Tooltip("Unique identifier for this request asset. Leave empty to auto-generate.")]
        public string RequestId;

        [Tooltip("Optional friendly name when referencing this request asset by name.")]
        public string DisplayName;

        [Header("Request")]
        public MethodKind Method = MethodKind.GET;

        [Tooltip("If true, 'PathOrUrl' is treated as a path (joined with the active environment BaseUrl). If false, it must be an absolute URL.")]
        public bool UseRelativePath = true;

        [Tooltip("Either a relative path like '/v1/profile' (UseRelativePath=true) or a full absolute URL.")]
        public string PathOrUrl = "/";

        [Tooltip("Optional request body (UTF-8). Common for POST/PUT/PATCH.")]
        [TextArea(3, 10)] public string Body;

        [Tooltip("Content-Type header for the request body, e.g., 'application/json'.")]
        public string BodyContentType = "application/json";

        [Header("Headers (optional)")]
        public List<Header> Headers = new();

        [Header("Policy")]
        public bool RequireAuth;
        public bool EnableCache;
        public int CacheTtlSeconds = 0;  // 0 = behavior default
        public bool CacheSWR = true;
        public bool DisableRetry;
        public string RateLimitKey;
        public bool UseIdempotencyKey;
        public bool CompressRequest;
        public bool DecompressResponse;
        public bool RecordRequest;

        [Serializable]
        public struct Header
        {
            public string Name;
            public string Value;
        }

        [Header("Notes (optional)")]
        [TextArea(2, 6)] public string Notes;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(RequestId))
            {
                RequestId = Guid.NewGuid().ToString("N");
            }

            if (string.IsNullOrEmpty(DisplayName))
            {
                DisplayName = name;
            }
        }
    }
}
