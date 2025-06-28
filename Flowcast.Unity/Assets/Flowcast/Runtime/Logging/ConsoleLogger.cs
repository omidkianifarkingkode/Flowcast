using System;

namespace Flowcast.Logging
{
    public class ConsoleLogger : ILogger
    {
        public void Log(string message) => Console.WriteLine($"[INFO] {message}");
        public void LogWarning(string message) => Console.WriteLine($"[WARN] {message}");
        public void LogError(string message) => Console.WriteLine($"[ERROR] {message}");
    }

}
