using System;
using UnityEngine;

namespace Flowcast.Network
{
    public interface INetworkBuilder
    {
        void UseDummyServer(DummyNetworkServerOptions options = default);
        void SetNetworkServices(INetworkConnectionService connectionService, INetworkCommandTransportService commandTransportService, INetworkGameStateSyncService syncService, INetworkRollbackService rollbackService, INetworkDiagnosticsService diagnosticsService);
        void SetNetworkManager(INetworkManager manager) => SetNetworkServices(manager, manager, manager, manager, manager);
    }

    public class NetworkBuilder : INetworkBuilder
    {
        public INetworkConnectionService ConnectionService { get; private set; }
        public INetworkCommandTransportService CommandTransportService { get; private set; }
        public INetworkGameStateSyncService SimulationSyncService { get; private set; }
        public INetworkRollbackService RollbackService { get; private set; }
        public INetworkDiagnosticsService DiagnosticsService { get; private set; }

        public void UseDummyServer(DummyNetworkServerOptions options = default)
        {
            var runner = new GameObject("Dummy Server", typeof(DummerServerRunner)).GetComponent<DummerServerRunner>();
            runner.Create(options);

            ConnectionService = runner.Server;
            CommandTransportService = runner.Server;
            SimulationSyncService = runner.Server;
            RollbackService = runner.Server;
            DiagnosticsService = runner.Server;
        }

        public void SetNetworkServices(INetworkConnectionService connectionService, INetworkCommandTransportService commandTransportService, INetworkGameStateSyncService syncService, INetworkRollbackService rollbackService, INetworkDiagnosticsService diagnosticsService)
        {
            ConnectionService = connectionService;
            CommandTransportService = commandTransportService;
            SimulationSyncService = syncService;
            RollbackService = rollbackService;
            DiagnosticsService = diagnosticsService;
        }

        public void SetNetworkManager(INetworkManager manager) => SetNetworkServices(manager, manager, manager, manager, manager);

        internal void Build()
        {
            if (ConnectionService == null) throw new InvalidOperationException("ConnectionService must be set.");
            if (CommandTransportService == null) throw new InvalidOperationException("CommandTransportService must be set.");
            if (SimulationSyncService == null) throw new InvalidOperationException("SimulationSyncService must be set.");
            if (RollbackService == null) throw new InvalidOperationException("RollbackService must be set.");
            if (DiagnosticsService == null) throw new InvalidOperationException("DiagnosticsService must be set.");
        }
    }
}
