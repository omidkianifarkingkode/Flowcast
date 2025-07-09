using System;
using UnityEngine;

namespace Flowcast.Network
{
    public interface INetworkBuilder
    {
        void UseDummyServer(DummyNetworkServerOptions options = default);
        void SetNetworkServices(INetworkConnectionService connectionService, INetworkCommandTransportService commandTransportService, INetworkGameStateSyncService syncService, INetworkDiagnosticsService diagnosticsService);
        void SetNetworkManager(INetworkManager manager) => SetNetworkServices(manager, manager, manager, manager);
    }

    public class NetworkBuilder : INetworkBuilder
    {
        public INetworkConnectionService ConnectionService { get; private set; }
        public INetworkCommandTransportService CommandTransportService { get; private set; }
        public INetworkGameStateSyncService SimulationSyncService { get; private set; }
        public INetworkDiagnosticsService DiagnosticsService { get; private set; }

        public void UseDummyServer(DummyNetworkServerOptions options = default)
        {
            var server = new GameObject("Dummy Server", typeof(DummyNetworkServer)).GetComponent<DummyNetworkServer>();

            server.Options = options;

            ConnectionService = server;
            CommandTransportService = server;
            SimulationSyncService = server;
            DiagnosticsService = server;
        }

        public void SetNetworkServices(INetworkConnectionService connectionService, INetworkCommandTransportService commandTransportService, INetworkGameStateSyncService syncService, INetworkDiagnosticsService diagnosticsService)
        {
            ConnectionService = connectionService;
            CommandTransportService = commandTransportService;
            SimulationSyncService = syncService;
            DiagnosticsService = diagnosticsService;
        }

        public void SetNetworkManager(INetworkManager manager) => SetNetworkServices(manager, manager, manager, manager);

        internal void Build()
        {
            if (ConnectionService == null) throw new InvalidOperationException("ConnectionService must be set.");
            if (CommandTransportService == null) throw new InvalidOperationException("CommandTransportService must be set.");
            if (SimulationSyncService == null) throw new InvalidOperationException("SimulationSyncService must be set.");
            if (DiagnosticsService == null) throw new InvalidOperationException("DiagnosticsService must be set.");
        }
    }
}
