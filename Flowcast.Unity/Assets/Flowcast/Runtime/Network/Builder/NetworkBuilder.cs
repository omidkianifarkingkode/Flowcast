using Flowcast.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Flowcast.Network
{
    public interface INetworkBuilder 
    {
        void UseDummyServer(DummyNetworkServerOptions options = default);
        void SetNetworkServices(INetworkConnectionService connectionService, INetworkInputTransportService inputTransportService, INetworkGameStateSyncService syncService, INetworkDiagnosticsService diagnosticsService);
        void SetNetworkManager(INetworkManager manager) => SetNetworkServices(manager, manager, manager, manager);
    }

    public class NetworkBuilder : INetworkBuilder
    {
        public INetworkConnectionService ConnectionService { get; private set; }
        public INetworkInputTransportService InputTransportService { get; private set; }
        public INetworkGameStateSyncService SimulationSyncService { get; private set; }
        public INetworkDiagnosticsService DiagnosticsService { get; private set; }

        public void UseDummyServer(DummyNetworkServerOptions options = default) 
        {
            var server = new GameObject("Dummy Server", typeof(DummyNetworkServer)).GetComponent<DummyNetworkServer>();

            server.Options = options;

            ConnectionService = server;
            InputTransportService = server;
            SimulationSyncService = server;
            DiagnosticsService = server;
        }

        public void SetNetworkServices(INetworkConnectionService connectionService, INetworkInputTransportService inputTransportService, INetworkGameStateSyncService syncService, INetworkDiagnosticsService diagnosticsService) 
        {
            ConnectionService = connectionService;
            InputTransportService = inputTransportService;
            SimulationSyncService = syncService;
            DiagnosticsService = diagnosticsService;
        }

        public void SetNetworkManager(INetworkManager manager) => SetNetworkServices(manager, manager, manager, manager);

        internal void Build()
        {
            if (ConnectionService == null) throw new InvalidOperationException("ConnectionService must be set.");
            if (InputTransportService == null) throw new InvalidOperationException("InputTransportService must be set.");
            if (SimulationSyncService == null) throw new InvalidOperationException("SimulationSyncService must be set.");
            if (DiagnosticsService == null) throw new InvalidOperationException("DiagnosticsService must be set.");
        }
    }
}
