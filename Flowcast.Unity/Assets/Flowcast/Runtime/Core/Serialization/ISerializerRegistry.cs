// Runtime/Core/Serialization/SerializerRegistry.cs
using System;
using System.Collections.Generic;

namespace Flowcast.Core.Serialization
{
    public interface ISerializerRegistry
    {
        void Register(ISerializer serializer, params string[] additionalMediaTypes);
        bool TryGet(string mediaType, out ISerializer serializer);
        ISerializer Default { get; }
    }

    public sealed class SerializerRegistry : ISerializerRegistry
    {
        private readonly Dictionary<string, ISerializer> _byMediaType =
            new(StringComparer.OrdinalIgnoreCase);

        public ISerializer Default { get; }

        public SerializerRegistry(ISerializer @default)
        {
            Default = @default ?? throw new ArgumentNullException(nameof(@default));
            Register(Default, Default.MediaType, "text/json", "application/*+json");
        }

        public void Register(ISerializer serializer, params string[] additionalMediaTypes)
        {
            if (serializer == null) return;
            if (!string.IsNullOrWhiteSpace(serializer.MediaType))
                _byMediaType[Normalize(serializer.MediaType)] = serializer;

            if (additionalMediaTypes != null)
            {
                foreach (var mt in additionalMediaTypes)
                    if (!string.IsNullOrWhiteSpace(mt))
                        _byMediaType[Normalize(mt)] = serializer;
            }
        }

        public bool TryGet(string mediaType, out ISerializer serializer)
        {
            serializer = null;
            if (string.IsNullOrWhiteSpace(mediaType)) return false;

            var norm = Normalize(mediaType);

            // exact
            if (_byMediaType.TryGetValue(norm, out serializer)) return true;

            // handle structured suffix e.g., application/vnd.foo+json
            var plus = norm.IndexOf('+');
            if (plus > 0)
            {
                var suffix = "application/*+" + norm.Substring(plus + 1);
                if (_byMediaType.TryGetValue(suffix, out serializer)) return true;
            }
            return false;
        }

        private static string Normalize(string mediaType)
        {
            var mt = mediaType.Trim().ToLowerInvariant();
            var semi = mt.IndexOf(';');
            if (semi >= 0) mt = mt.Substring(0, semi).Trim();
            return mt;
        }
    }
}
