using LogKit;
using System.Collections.Generic;

namespace Flowcast.Commands
{
    public static class LoggerExtensions
    {
        public static void LogCommandDispatch(this ILogger logger, IReadOnlyCollection<ICommand> commands, string prefix = "[CommandDispatch]")
        {
            if (!logger.IsEnabled || commands == null || commands.Count == 0)
                return;

            var lines = new List<string>();
            int index = 1;

            foreach (var command in commands)
            {
                if (command == null) continue;

                string summary = command.ToString();
                string type = command.GetType().Name;

                string line = $"[{index}] {type} : {summary} | id={command.Id}, playerId={command.PlayerId}, frame={command.Frame}";
                lines.Add(line);
                index++;
            }

            string message = $"{prefix} Sent {commands.Count} command(s):\n" + string.Join("\n", lines);
            logger.Log(message, LogLevel.Info);
        }

        public static void LogRemoteCommandReceive(this ILogger logger, IReadOnlyCollection<ICommand> commands, string prefix = "[RemoteCommands]")
        {
            if (!logger.IsEnabled || commands == null || commands.Count == 0)
                return;

            var lines = new List<string>();
            int index = 1;

            foreach (var command in commands)
            {
                if (command == null) continue;

                string type = command.GetType().Name;
                string summary = command.ToString();
                string line = $"{index}. {type} : {summary} | id={command.Id}, playerId={command.PlayerId}, frame={command.Frame}";

                lines.Add(line);
                index++;
            }

            string message = $"{prefix} Received {commands.Count} command(s):\n" + string.Join("\n", lines);
            logger.Log(message, LogLevel.Info);
        }
    }
}
