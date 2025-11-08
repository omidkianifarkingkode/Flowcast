// Runtime/Core/Common/Result/Headers.cs
using System.Collections.Generic;

namespace Flowcast.Core.Common
{
    public sealed class Headers
    {
        private readonly Dictionary<string, string> _map =
            new(System.StringComparer.OrdinalIgnoreCase);

        public void Set(string name, string value) { if (!string.IsNullOrEmpty(name)) _map[name] = value ?? string.Empty; }
        public bool TryGet(string name, out string value) => _map.TryGetValue(name, out value);
        public IEnumerable<KeyValuePair<string, string>> Pairs => _map;
    }
}
