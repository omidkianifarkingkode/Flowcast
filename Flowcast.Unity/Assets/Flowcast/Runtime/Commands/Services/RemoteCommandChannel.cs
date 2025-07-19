using Flowcast.Commands;
using Flowcast.Synchronization;
using System;
using System.Collections.Generic;

namespace Flowcast.Network
{
    public class RemoteCommandChannel : IRemoteCommandChannel
    {
        private readonly Dictionary<ulong, List<ICommand>> _commandBuffer = new();
        private readonly INetworkCommandTransportService _networkService;

        public RemoteCommandChannel(INetworkCommandTransportService commandTransportService)
        {
            _networkService = commandTransportService;
            _networkService.OnCommandsReceived += ReceiveCommands;
        }

        public event Action<IReadOnlyCollection<ICommand>> OnCommandsReceived;
        public event Action<IReadOnlyCollection<ICommand>> OnCommandsSent;

        public void SendCommands(IReadOnlyCollection<ICommand> commands)
        {
            _networkService.SendCommands(commands);

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

        public void ResetWith(IReadOnlyCollection<ICommand> commands)
        {
            _commandBuffer.Clear();
            AddCommands(commands);
        }

        private void ReceiveCommands(IReadOnlyCollection<ICommand> commands)
        {
            AddCommands(commands);

            OnCommandsReceived?.Invoke(commands);
        }

        private void AddCommands(IReadOnlyCollection<ICommand> commands)
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
        }
    }
}
