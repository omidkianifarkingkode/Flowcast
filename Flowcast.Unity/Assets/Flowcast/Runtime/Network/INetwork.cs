using Flowcast.Data;
using Flowcast.Inputs;
using NUnit.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flowcast.Network
{
    public interface INetworkConnectionService : IDisposable
    {
        event System.Action OnConnected;
        event System.Action OnDisconnected;
        event Action<Exception> OnConnectionError;

        void Connect(string serverAddress);
        void Disconnect();
        bool IsConnected { get; }
        TimeSpan EstimatedLatency { get; }
        void Tick();
    }

    public interface IMatchmakingService
    {
        Task RequestMatchAsync(string gameMode, object customData = null);
        event Action<GameSessionData> OnMatchFound;
    }

    public interface IInputTransportService
    {
        void SendInputs(IReadOnlyCollection<IInput> inputs);

        event Action<IReadOnlyCollection<IInput>> OnInputsReceived;
    }

    public interface ISimulationSyncService
    {
        void SendStateHash(ulong frame, uint hash);
        event Action<ulong /*frame*/, bool /*isSynced*/> OnSyncStatusReceived;
        event Action<ulong /*rollbackToFrame*/> OnRollbackRequested;
    }

    public interface INetworkDiagnosticsService
    {
        void SendPing();
        event Action<TimeSpan> OnPingResult;
    }


    /// <summary>
    /// var bitset = new BitSet();
    /// bitset.Set(0); // Mark frame 100 as received
    /// bitset.Set(1); // Mark frame 99
    /// bool isReceived = bitset.IsSet(0); // true
    /// </summary>
    public class BitSet
    {
        private uint _bits;

        public void Set(int index) => _bits |= (1u << index);
        public void Clear(int index) => _bits &= ~(1u << index);
        public bool IsSet(int index) => (_bits & (1u << index)) != 0;

        public uint Value => _bits;
        public void FromValue(uint value) => _bits = value;
    }


    public interface INetworkManager :
        INetworkConnectionService,
        IMatchmakingService,
        IInputTransportService,
        ISimulationSyncService,
        INetworkDiagnosticsService
    { }

}
