using Flowcast.Network;
using Flowcast.Serialization;

namespace Flowcast.Builders
{
    public interface IRequireGameState
    {
        IRequireNetwork SetGameStateModel(ISerializableGameState state);
    }

    public interface IRequireNetwork 
    {
        public IOptionalSettings SetNetworkServices(
            INetworkConnectionService connectionService,
            IInputTransportService inputTransportService,
            ISimulationSyncService syncService,
            INetworkDiagnosticsService diagnosticsService);

        public IOptionalSettings SetNetworkManager(INetworkManager manager) => SetNetworkServices(manager, manager, manager, manager);
    }
}
