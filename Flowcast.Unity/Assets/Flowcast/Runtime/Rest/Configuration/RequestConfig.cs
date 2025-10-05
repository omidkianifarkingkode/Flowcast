using System;
using System.Collections.Generic;

namespace Flowcast.Rest.Configuration
{
    public class RequestConfig
    {
        public string Route { get; set; }
        public string Method { get; set; } = "GET";
        public int? CacheDuration { get; set; }
        public bool RequireAuth { get; set; }
    }

    public class RequestConfigRegistry
    {
        private readonly Dictionary<Type, RequestConfig> _configs = new();

        public void Configure<TRequest>(Action<RequestConfig> configAction)
        {
            var cfg = new RequestConfig();
            configAction(cfg);
            _configs[typeof(TRequest)] = cfg;
        }

        public RequestConfig GetConfig(Type type)
        {
            return _configs.TryGetValue(type, out var cfg) ? cfg : null;
        }
    }
}
