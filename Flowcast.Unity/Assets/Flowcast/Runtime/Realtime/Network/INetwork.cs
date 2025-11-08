using Flowcast.Commands;
using Flowcast.Data;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Flowcast.Network
{
    public interface INetworkConnectionService : IDisposable
    {
        event Action OnConnected;
        event Action OnDisconnected;
        event Action<Exception> OnConnectionError;

        void Connect(string serverAddress);
        void Disconnect();
        bool IsConnected { get; }
        TimeSpan EstimatedLatency { get; }
    }

    public interface INetworkMatchmakingService
    {
        Task RequestMatchAsync(string gameMode, object customData = null);
        event Action<MatchInfo> OnMatchFound;
    }

    public interface INetworkCommandTransportService
    {
        void SendCommands(IReadOnlyCollection<ICommand> commands);

        event Action<IReadOnlyCollection<ICommand>> OnCommandsReceived;
    }

    public interface INetworkGameStateSyncService
    {
        void SendStateHash(StateHashReport report);

        event Action<SyncStatus> OnSyncStatusReceived;
    }

    public interface INetworkRollbackService
    {
        void RequestCommandsHistory();

        event Action<RollbackRequest> OnRollbackRequested;
        event Action<IReadOnlyCollection<ICommand>> OnCommandsHistoryReceived;
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
        INetworkMatchmakingService,
        INetworkCommandTransportService,
        INetworkGameStateSyncService,
        INetworkRollbackService,
        INetworkDiagnosticsService
    { }
}
