using Flowcast.Inputs;
using System;
using System.Collections.Generic;

namespace Flowcast.Network
{
    public class RemoteInputCollector : IRemoteInputCollector
    {
        private readonly Dictionary<ulong, List<IInput>> _inputBuffer = new();

        public event Action<IReadOnlyCollection<IInput>> OnInputsReceived;

        public void SendInputs(IReadOnlyCollection<IInput> inputs)
        {
            // Send over the network to server...
            // Your implementation
        }

        public void ReceiveInputs(IReadOnlyCollection<IInput> inputs)
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
    }
}
