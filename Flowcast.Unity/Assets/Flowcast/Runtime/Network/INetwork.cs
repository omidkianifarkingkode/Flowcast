using NUnit.Framework.Interfaces;
using System;
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


    public interface IGameStateVerificationService
    {
        void SendGameStateHash(ulong frameNumber, byte[] gameStateHash, BitSet inputAckBitmap);
        event Action<string> OnStatusReceived;
    }

    public class BitSet
    {
    }

    public interface IRemoteCommandReceiver
    {
        event Action<ulong> OnRollbackRequested;
    }

    public interface INetworkManager :
        INetworkConnectionService,
        IGameStateVerificationService,
        IRemoteCommandReceiver
    {
    }
}
