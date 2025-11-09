// Editor/Rest/Workbench/CurlImportUtility.cs
#if UNITY_EDITOR
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Flowcast.Rest.Workbench;

namespace Flowcast.Rest.Editor
{
    public static class CurlImportUtility
    {
        // Supports: curl -X, -H/--header, --data/--data-raw/--data-binary, -d, --compressed, -i ignored
        public static bool TryParse(string curl, out RequestAsset.MethodKind method, out string url, out List<RequestAsset.Header> headers, out string body, out string contentType)
        {
            method = RequestAsset.MethodKind.GET;
            url = "";
            headers = new();
            body = "";
            contentType = "application/json";

            if (string.IsNullOrWhiteSpace(curl) || !curl.TrimStart().StartsWith("curl")) return false;

            var tokens = Tokenize(curl);
            for (int i = 0; i < tokens.Count; i++)
            {
                var t = tokens[i];

                if (t == "-X" || t == "--request")
                {
                    if (i + 1 < tokens.Count)
                    {
                        var m = tokens[++i].ToUpperInvariant();
                        if (System.Enum.TryParse<RequestAsset.MethodKind>(m, out var mk)) method = mk;
                    }
                }
                else if (t == "-H" || t == "--header")
                {
                    if (i + 1 < tokens.Count)
                    {
                        var hv = tokens[++i];
                        var idx = hv.IndexOf(':');
                        if (idx > 0)
                        {
                            var name = hv.Substring(0, idx).Trim();
                            var value = hv.Substring(idx + 1).Trim();
                            headers.Add(new RequestAsset.Header { Name = name, Value = value });
                            if (name.Equals("Content-Type", System.StringComparison.OrdinalIgnoreCase))
                                contentType = value;
                        }
                    }
                }
                else if (t == "--data" || t == "--data-raw" || t == "--data-binary" || t == "-d")
                {
                    if (i + 1 < tokens.Count) body = tokens[++i];
                }
                else if (t.StartsWith("http://") || t.StartsWith("https://"))
                {
                    url = t;
                }
                else if (t == "--compressed" || t == "-i" || t == "-s" || t == "--silent")
                {
                    // ignore
                }
            }

            // If method not explicitly set and body present, default to POST
            if ((method == RequestAsset.MethodKind.GET || method == RequestAsset.MethodKind.HEAD) && !string.IsNullOrEmpty(body))
                method = RequestAsset.MethodKind.POST;

            return !string.IsNullOrEmpty(url);
        }

        // naive tokenizer respecting single/double quotes
        private static List<string> Tokenize(string s)
        {
            var list = new List<string>();
            var rgx = new Regex(@"('([^']*)'|""([^""]*)""|[^\s]+)", RegexOptions.Compiled);
            foreach (Match m in rgx.Matches(s))
            {
                var val = m.Value;
                if (val.Length >= 2 && ((val.StartsWith("'") && val.EndsWith("'")) || (val.StartsWith("\"") && val.EndsWith("\""))))
                    val = val.Substring(1, val.Length - 2);
                list.Add(val);
            }
            return list;
        }
    }
}
#endif
