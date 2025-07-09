using Flowcast.Commands;
using System;
using System.Collections.Generic;

namespace Flowcast.Network
{
    public class RemoteCommandChannel : IRemoteCommandChannel
    {
        private readonly Dictionary<ulong, List<ICommand>> _commandBuffer = new();
        private readonly INetworkCommandTransportService _commandTransportService;

        public RemoteCommandChannel(INetworkCommandTransportService commandTransportService)
        {
            _commandTransportService = commandTransportService;
            _commandTransportService.OnCommandsReceived += ReceiveCommands;
        }

        public event Action<IReadOnlyCollection<ICommand>> OnCommandsReceived;
        public event Action<IReadOnlyCollection<ICommand>> OnCommandsSent;

        public void SendCommands(IReadOnlyCollection<ICommand> commands)
        {
            _commandTransportService.SendCommands(commands);

            OnCommandsSent?.Invoke(commands);
        }

        public IReadOnlyCollection<ICommand> GetCommandsForFrame(ulong frame)
        {
            if (_commandBuffer.TryGetValue(frame, out var commands))
                return commands;

            return Array.Empty<ICommand>();
        }

        public void RemoveCommandsForFrame(ulong frame)
        {
            _commandBuffer.Remove(frame);
        }

        private void ReceiveCommands(IReadOnlyCollection<ICommand> commands)
        {
            foreach (var command in commands)
            {
                if (!_commandBuffer.TryGetValue(command.Frame, out var list))
                {
                    list = new List<ICommand>();
                    _commandBuffer[command.Frame] = list;
                }
                list.Add(command);
            }

            OnCommandsReceived?.Invoke(commands);
        }
    }
}
