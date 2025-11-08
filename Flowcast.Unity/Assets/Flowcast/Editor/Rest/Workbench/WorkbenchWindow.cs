// Editor/Rest/Workbench/WorkbenchWindow.cs
#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Flowcast.Core.Cache;
using Flowcast.Core.Environments;
using Flowcast.Core.Serialization;
using Flowcast.Rest.Client;
using Flowcast.Rest.Pipeline;
using Flowcast.Rest.Transport;
using Flowcast.Rest.Workbench;
using UnityEditor;
using UnityEngine;
using Environment = Flowcast.Core.Environments.Environment;

namespace Flowcast.Rest.Editor
{
    public sealed class WorkbenchWindow : EditorWindow
    {
        [MenuItem("Flowcast/REST Workbench")]
        private static void Open() => GetWindow<WorkbenchWindow>("Flowcast REST");

        // --- UI State ---
        private RequestAsset.MethodKind _method = RequestAsset.MethodKind.GET;
        private bool _useRelative = true;
        private string _pathOrUrl = "/v1/health";
        private string _body = "";
        private string _contentType = "application/json";
        private readonly List<RequestAsset.Header> _headers = new();

        private TransportMode _transportMode = TransportMode.Real;

        // Mock response fields (used only when TransportMode.Mock)
        private int _mockStatus = 200;
        private int _mockLatencyMs = 0;
        private string _mockMediaType = "application/json";
        private string _mockBody = "{\"ok\":true}";

        // Saved asset
        private RequestAsset _loadedAsset;

        // Environment set
        private FlowcastClientSettings _settings;
        private int _envIndex = 0;
        private string[] _envDisplayNames = Array.Empty<string>();
        private Environment[] _envRefs = Array.Empty<Environment>();

        // Response view
        private string _respStatus = "";
        private string _respMedia = "";
        private List<KeyValuePair<string, string>> _respHeaders = new();
        private string _respBody = "";
        private Vector2 _reqScroll;
        private Vector2 _respScroll;

        private Vector2 _savedScroll;
        private List<RequestAsset> _savedAssets = new();
        private string _savedSearch = "";
        private double _lastSearchRefresh;

        private CancellationTokenSource _cts;

        private enum TransportMode { Real, Mock }

        private void OnEnable()
        {
            _settings = FlowcastClientSettings.LoadFromResources();
            RebuildEnvList();
        }

        private void OnDisable()
        {
            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        private void RebuildEnvList()
        {
            if (_settings?.EnvironmentSet == null || _settings.EnvironmentSet.Environments.Count == 0)
            {
                _envDisplayNames = new[] { "(no EnvironmentSet)" };
                _envRefs = Array.Empty<Environment>();
                _envIndex = 0;
                return;
            }

            var list = _settings.EnvironmentSet.Environments;
            _envRefs = new Environment[list.Count];
            _envDisplayNames = new string[list.Count];
            for (int i = 0; i < list.Count; i++)
            {
                _envRefs[i] = list[i];
                _envDisplayNames[i] = list[i] != null ? $"{list[i].DisplayName} ({list[i].Id})" : "(null)";
            }

            // select current if possible
            var current = EnvironmentProvider.Instance.Current;
            if (current != null)
            {
                var idx = Array.FindIndex(_envRefs, e => e == current);
                if (idx >= 0) _envIndex = idx;
            }
        }

        private void OnGUI()
        {
            DrawHeader();
            EditorGUILayout.Space(8);
            EditorGUILayout.BeginHorizontal();
            DrawRequestPanel();
            GUILayout.Space(8);
            DrawResponsePanel();
            EditorGUILayout.EndHorizontal();
        }

        private void DrawHeader()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            GUILayout.Label("Environment:", GUILayout.Width(90));

            using (new EditorGUI.DisabledScope(_envRefs.Length == 0))
            {
                var newIdx = EditorGUILayout.Popup(_envIndex, _envDisplayNames);
                if (newIdx != _envIndex && newIdx >= 0 && newIdx < _envRefs.Length)
                {
                    _envIndex = newIdx;
                    if (_envRefs[_envIndex] != null)
                        EnvironmentProvider.Instance.Set(_envRefs[_envIndex]);
                }
            }

            GUILayout.FlexibleSpace();

            GUILayout.Label("Transport:", GUILayout.Width(75));
            _transportMode = (TransportMode)EditorGUILayout.EnumPopup(_transportMode, GUILayout.Width(90));
            EditorGUILayout.EndHorizontal();
        }

        private void DrawRequestPanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.48f));

            EditorGUILayout.LabelField("Request", EditorStyles.boldLabel);
            _reqScroll = EditorGUILayout.BeginScrollView(_reqScroll, GUILayout.MinHeight(240));

            // Method + Relative toggle
            EditorGUILayout.BeginHorizontal();
            _method = (RequestAsset.MethodKind)EditorGUILayout.EnumPopup(_method, GUILayout.Width(90));
            GUILayout.Space(8);
            _useRelative = EditorGUILayout.ToggleLeft("Use relative path", _useRelative, GUILayout.Width(150));
            EditorGUILayout.EndHorizontal();

            // Path/URL
            EditorGUILayout.LabelField(_useRelative ? "Path" : "Absolute URL");
            _pathOrUrl = EditorGUILayout.TextField(_pathOrUrl);

            // Headers grid
            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Headers");
            DrawHeadersGrid(_headers);

            // Body (only for methods that typically have bodies)
            if (_method is RequestAsset.MethodKind.POST or RequestAsset.MethodKind.PUT or RequestAsset.MethodKind.PATCH)
            {
                EditorGUILayout.Space(6);
                EditorGUILayout.LabelField("Body (UTF-8)");
                _body = EditorGUILayout.TextArea(_body, GUILayout.MinHeight(80));

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Content-Type", GUILayout.Width(90));
                _contentType = EditorGUILayout.TextField(_contentType);
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();

            // Mock section
            if (_transportMode == TransportMode.Mock)
            {
                EditorGUILayout.Space(8);
                EditorGUILayout.LabelField("Mock Response (when using Mock transport)", EditorStyles.boldLabel);
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Status", GUILayout.Width(60));
                _mockStatus = EditorGUILayout.IntField(_mockStatus, GUILayout.Width(60));
                GUILayout.Label("Latency ms", GUILayout.Width(80));
                _mockLatencyMs = EditorGUILayout.IntField(_mockLatencyMs, GUILayout.Width(60));
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label("Media Type", GUILayout.Width(80));
                _mockMediaType = EditorGUILayout.TextField(_mockMediaType);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.LabelField("Body (UTF-8)");
                _mockBody = EditorGUILayout.TextArea(_mockBody, GUILayout.MinHeight(60));
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.Space(8);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Send", GUILayout.Height(28))) _ = SendAsync();
            if (GUILayout.Button("Clear Response", GUILayout.Height(28))) ClearResponse();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Save as Asset", GUILayout.Height(28))) SaveAsAsset();
            EditorGUI.BeginDisabledGroup(_loadedAsset == null);
            if (GUILayout.Button("Update Asset", GUILayout.Height(28))) UpdateAsset();
            EditorGUI.EndDisabledGroup();
            if (GUILayout.Button("Copy as cURL", GUILayout.Height(28)))
            {
                EditorGUIUtility.systemCopyBuffer = BuildCurl();
                ShowNotification(new GUIContent("cURL copied"));
            }
            EditorGUILayout.EndHorizontal();

            // Load existing
            EditorGUILayout.Space(6);
            EditorGUILayout.BeginHorizontal();
            _loadedAsset = (RequestAsset)EditorGUILayout.ObjectField("Loaded Asset", _loadedAsset, typeof(RequestAsset), false);
            if (GUILayout.Button("Load", GUILayout.Width(60))) LoadFromAsset();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Run Saved", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Search", GUILayout.Width(50));
            var newSearch = EditorGUILayout.TextField(_savedSearch);
            if (newSearch != _savedSearch) { _savedSearch = newSearch; RefreshSavedAssets(force: true); }
            if (GUILayout.Button("Refresh", GUILayout.Width(80))) RefreshSavedAssets(force: true);
            EditorGUILayout.EndHorizontal();

            _savedScroll = EditorGUILayout.BeginScrollView(_savedScroll, GUILayout.MinHeight(140));
            RefreshSavedAssets();
            if (_savedAssets.Count == 0)
            {
                EditorGUILayout.LabelField("(no RequestAsset files found)");
            }
            else
            {
                foreach (var a in _savedAssets)
                {
                    if (a == null) continue;
                    var name = a.name;
                    if (!string.IsNullOrEmpty(_savedSearch) && !name.Contains(_savedSearch, StringComparison.OrdinalIgnoreCase))
                        continue;

                    EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                    EditorGUILayout.ObjectField(a, typeof(RequestAsset), false);
                    GUILayout.FlexibleSpace();
                    if (GUILayout.Button("Load", GUILayout.Width(70)))
                    {
                        _loadedAsset = a;
                        LoadFromAsset();
                    }
                    if (GUILayout.Button("Send", GUILayout.Width(70)))
                    {
                        _loadedAsset = a;
                        LoadFromAsset();
                        _ = SendAsync();
                    }
                    EditorGUILayout.EndHorizontal();
                }
            }
            EditorGUILayout.EndScrollView();


            EditorGUILayout.EndVertical();
        }

        private void DrawResponsePanel()
        {
            EditorGUILayout.BeginVertical(GUILayout.Width(position.width * 0.48f));
            EditorGUILayout.LabelField("Response", EditorStyles.boldLabel);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Status", _respStatus);
            EditorGUILayout.LabelField("Media Type", _respMedia);

            EditorGUILayout.Space(4);
            EditorGUILayout.LabelField("Headers");
            if (_respHeaders.Count == 0) EditorGUILayout.LabelField("(none)");
            else
            {
                foreach (var kv in _respHeaders)
                    EditorGUILayout.LabelField(kv.Key, kv.Value);
            }

            EditorGUILayout.Space(6);
            EditorGUILayout.LabelField("Body");
            _respScroll = EditorGUILayout.BeginScrollView(_respScroll, GUILayout.MinHeight(180));
            EditorGUILayout.SelectableLabel(_respBody ?? "", GUILayout.ExpandHeight(true));
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();

            EditorGUILayout.EndVertical();
        }

        private void DrawHeadersGrid(List<RequestAsset.Header> list)
        {
            // Table-ish with + and - controls
            int toRemove = -1;
            for (int i = 0; i < list.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                list[i] = new RequestAsset.Header
                {
                    Name = EditorGUILayout.TextField(list[i].Name, GUILayout.Width(position.width * 0.18f)),
                    Value = EditorGUILayout.TextField(list[i].Value)
                };
                if (GUILayout.Button("-", GUILayout.Width(24))) toRemove = i;
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("+ Add header", GUILayout.Width(120))) list.Add(new RequestAsset.Header());
            EditorGUILayout.EndHorizontal();
            if (toRemove >= 0 && toRemove < list.Count) list.RemoveAt(toRemove);
        }

        // --- Actions ---

        private async Task SendAsync()
        {
            ClearResponse();

            var envProv = EnvironmentProvider.Instance;
            var serializers = new SerializerRegistry(new UnityJsonSerializer());
            var cache = new MemoryCacheProvider(128);

            ITransport transport;
            if (_transportMode == TransportMode.Real)
            {
                transport = new UnityWebRequestTransport();
            }
            else
            {
                var mock = new MockTransport();
                // Register an inline mock for THIS request if no dedicated route was set externally.
                var resp = new MockResponse
                {
                    Status = _mockStatus,
                    LatencyMs = _mockLatencyMs,
                    MediaType = _mockMediaType,
                    BodyBytes = Encoding.UTF8.GetBytes(_mockBody ?? "")
                };

                var abs = ResolveAbsoluteUrl();
                mock.When(MethodToString(_method), abs, resp);
                transport = mock;
            }

            var client = new RestClient(
                envProvider: envProv,
                transport: transport,
                serializers: serializers,
                auth: null,
                addDefaultLogging: false);

            // Recommended behavior order
            client.AddBehavior(new CacheBehavior(cache));
            // (AuthBehavior skipped in WB by default; you can add later)
            client.AddBehavior(new RetryBehavior());
            client.AddBehavior(new LoggingBehavior(envProv));

            // Build request
            var builder = client.Send(MethodToString(_method), _useRelative ? _pathOrUrl : ResolveAbsoluteUrl());

            foreach (var h in _headers)
            {
                if (!string.IsNullOrWhiteSpace(h.Name))
                    builder = builder.WithHeader(h.Name, h.Value ?? "");
            }

            if (_method is RequestAsset.MethodKind.POST or RequestAsset.MethodKind.PUT or RequestAsset.MethodKind.PATCH)
            {
                var bytes = Encoding.UTF8.GetBytes(_body ?? "");
                builder = builder.WithBytesBody(bytes, string.IsNullOrWhiteSpace(_contentType) ? "application/octet-stream" : _contentType);
            }

            // Fire
            try
            {
                _cts?.Cancel(); _cts?.Dispose();
                _cts = new CancellationTokenSource();

                var result = await builder.AsRawAsync(_cts.Token); // raw keeps it format-agnostic
                if (result.IsSuccess)
                {
                    var raw = result.Value;
                    _respStatus = $"{raw.Status}";
                    _respMedia = raw.MediaType ?? "";
                    _respHeaders = new List<KeyValuePair<string, string>>(raw.Headers.Pairs);
                    _respBody = BodyPreview(raw);
                }
                else
                {
                    _respStatus = $"ERROR ({result.Error.Kind}) {result.Error.Status}";
                    _respMedia = "text/plain";
                    _respHeaders = new();
                    _respBody = result.Error.Message;
                }
            }
            catch (Exception ex)
            {
                _respStatus = "EXCEPTION";
                _respMedia = "text/plain";
                _respHeaders = new();
                _respBody = ex.ToString();
            }

            Repaint();
        }

        private string ResolveAbsoluteUrl()
        {
            if (!_useRelative) return _pathOrUrl ?? "";
            var env = EnvironmentProvider.Instance.Current;
            var baseUrl = env != null ? env.BaseUrl ?? "" : "";
            if (!baseUrl.EndsWith("/")) baseUrl += "/";
            var path = _pathOrUrl ?? "";
            if (path.StartsWith("/")) path = path.Substring(1);
            return baseUrl + path;
        }

        private static string MethodToString(RequestAsset.MethodKind m) => m.ToString().ToUpperInvariant();

        private static string BodyPreview(Flowcast.Core.Common.RawResponse raw)
        {
            if (raw.BodyBytes == null || raw.BodyBytes.Length == 0) return "(empty)";
            // Show as UTF-8 text by default; if it's obviously binary, show length.
            var media = raw.MediaType?.ToLowerInvariant() ?? "";
            if (media.Contains("json") || media.StartsWith("text/") || media.Contains("xml"))
            {
                try
                {
                    return Encoding.UTF8.GetString(raw.BodyBytes);
                }
                catch { }
            }
            return $"[{raw.BodyBytes.Length} bytes]";
        }

        private void ClearResponse()
        {
            _respStatus = "";
            _respMedia = "";
            _respHeaders = new();
            _respBody = "";
        }

        private void SaveAsAsset()
        {
            var asset = ScriptableObject.CreateInstance<RequestAsset>();
            asset.Method = _method;
            asset.UseRelativePath = _useRelative;
            asset.PathOrUrl = _pathOrUrl;
            asset.Body = _body;
            asset.BodyContentType = _contentType;
            asset.Headers = new List<RequestAsset.Header>(_headers);

            var path = EditorUtility.SaveFilePanelInProject("Save Request Asset", "RequestAsset", "asset", "Choose a location for the request asset");
            if (!string.IsNullOrEmpty(path))
            {
                AssetDatabase.CreateAsset(asset, path);
                AssetDatabase.SaveAssets();
                _loadedAsset = asset;
                EditorGUIUtility.PingObject(asset);
            }
        }

        private void UpdateAsset()
        {
            if (_loadedAsset == null) return;
            Undo.RecordObject(_loadedAsset, "Update RequestAsset");
            _loadedAsset.Method = _method;
            _loadedAsset.UseRelativePath = _useRelative;
            _loadedAsset.PathOrUrl = _pathOrUrl;
            _loadedAsset.Body = _body;
            _loadedAsset.BodyContentType = _contentType;
            _loadedAsset.Headers = new List<RequestAsset.Header>(_headers);
            EditorUtility.SetDirty(_loadedAsset);
            AssetDatabase.SaveAssets();
        }

        private void LoadFromAsset()
        {
            if (_loadedAsset == null) return;
            _method = _loadedAsset.Method;
            _useRelative = _loadedAsset.UseRelativePath;
            _pathOrUrl = _loadedAsset.PathOrUrl;
            _body = _loadedAsset.Body;
            _contentType = _loadedAsset.BodyContentType;
            _headers.Clear();
            if (_loadedAsset.Headers != null) _headers.AddRange(_loadedAsset.Headers);
        }

        private string BuildCurl()
        {
            var abs = ResolveAbsoluteUrl();
            var sb = new System.Text.StringBuilder();
            sb.Append("curl -i -X ").Append(MethodToString(_method)).Append(' ');
            // headers
            foreach (var h in _headers)
            {
                if (string.IsNullOrWhiteSpace(h.Name)) continue;
                sb.Append("--header ").Append('"').Append(h.Name).Append(": ").Append(h.Value ?? "").Append('"').Append(' ');
            }
            // body
            if (_method is RequestAsset.MethodKind.POST or RequestAsset.MethodKind.PUT or RequestAsset.MethodKind.PATCH)
            {
                if (!string.IsNullOrWhiteSpace(_contentType))
                    sb.Append("--header ").Append('"').Append("Content-Type: ").Append(_contentType).Append('"').Append(' ');
                var bodyEsc = (_body ?? "").Replace("\\", "\\\\").Replace("\"", "\\\"");
                sb.Append("--data ").Append('"').Append(bodyEsc).Append('"').Append(' ');
            }
            sb.Append('"').Append(abs).Append('"');
            return sb.ToString();
        }

        private void RefreshSavedAssets(bool force = false)
        {
            // Throttle a bit to avoid AssetDatabase spam
            if (!force && EditorApplication.timeSinceStartup - _lastSearchRefresh < 1.0) return;
            _lastSearchRefresh = EditorApplication.timeSinceStartup;

            _savedAssets.Clear();
            var guids = AssetDatabase.FindAssets("t:Flowcast.Rest.Workbench.RequestAsset");
            foreach (var g in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(g);
                var asset = AssetDatabase.LoadAssetAtPath<RequestAsset>(path);
                if (asset != null) _savedAssets.Add(asset);
            }
        }

    }
}
#endif
