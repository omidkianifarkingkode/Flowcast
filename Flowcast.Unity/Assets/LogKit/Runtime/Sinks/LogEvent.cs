using System;
using System.Collections.Generic;

namespace LogKit.Sinks
{
    public class LogEvent
    {
        public DateTime Timestamp { get; set; } = DateTime.Now;
        public string MessageTemplate { get; set; }
        public LogLevel Level { get; set; }
        public Exception Exception { get; set; }
        public Dictionary<string, object> Properties { get; set; } = new();
    }
}
