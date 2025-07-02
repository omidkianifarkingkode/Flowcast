using Flowcast.Inputs;
using System;
using System.Collections.Generic;

namespace Flowcast.Network
{
    public class RemoteInputChannel : IRemoteInputChannel
    {
        private readonly Dictionary<ulong, List<IInput>> _inputBuffer = new();
        private readonly IInputTransportService _inputTransportService;

        public RemoteInputChannel(IInputTransportService inputTransportService)
        {
            _inputTransportService = inputTransportService;
            _inputTransportService.OnInputsReceived += ReceiveInputs;
        }

        public event Action<IReadOnlyCollection<IInput>> OnInputsReceived;

        public void SendInputs(IReadOnlyCollection<IInput> inputs)
        {
            _inputTransportService.SendInputs(inputs);
        }

        public IReadOnlyCollection<IInput> GetInputsForFrame(ulong frame)
        {
            if (_inputBuffer.TryGetValue(frame, out var inputs))
                return inputs;
            return Array.Empty<IInput>();
        }

        public void RemoveInputsForFrame(ulong frame)
        {
            _inputBuffer.Remove(frame);
        }

        private void ReceiveInputs(IReadOnlyCollection<IInput> inputs)
        {
            foreach (var input in inputs)
            {
                if (!_inputBuffer.TryGetValue(input.Frame, out var list))
                {
                    list = new List<IInput>();
                    _inputBuffer[input.Frame] = list;
                }
                list.Add(input);
            }

            OnInputsReceived?.Invoke(inputs); // optional
        }
    }
}
