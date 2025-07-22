using Flowcast.Commons;
using Flowcast.Network;
using Flowcast.Rollback;
using Flowcast.Serialization;
using Flowcast.Synchronization;
using LogKit;
using LogKit.Bootstrap;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace Flowcast.Tests.Runtime.GameSynchronizationTests
{

    [TestFixture]
    public class SnapshotBufferTests
    {
        private SampleGameState _gameState;
        private SnapshotRepository _snapshotRepository;
        private RollbackHandler _rollbackHandler;
        private IGameStateSerializer<SampleGameState> _serializer;
        private DummyNetworkServer _network;
        private GameStateSyncOptions _options;

        private const int SnapshotLimit = 10;

        [SetUp]
        public void SetUp()
        {
            _gameState = new SampleGameState();

            //_serializer = new JsonSerializer<SampleGameState>(() => _gameState, default);
            _serializer = new BinarySerializer<SampleGameState>(() => _gameState);
            _network = new DummyNetworkServer();
            _options = new GameStateSyncOptions
            {
                SnapshotHistoryLimit = SnapshotLimit,
                OnRollback = HandleRollback
            };
            var hasher = new XorHasher();
            LoggerBootstrapper.Initialize();
            var logger = LoggerFactory.Create("Snapshot");

            _snapshotRepository = new SnapshotRepository(_serializer, hasher, _network, _options, logger);
            _rollbackHandler = new RollbackHandler(_serializer, _snapshotRepository, _network, _options, logger);
        }

        [Test]
        public void CaptureSnapshot_AddsEntryToBuffer()
        {
            _gameState.HP = 100;
            _gameState.Gold = 250;

            _snapshotRepository.CaptureAndSyncSnapshot(10);

            Assert.IsTrue(_snapshotRepository.TryGetSnapshot(10, out var snapshot));
            var state = _serializer.DeserializeSnapshot(snapshot.Data);
            Assert.AreEqual(100, state.HP);
            Assert.AreEqual(250, state.Gold);
        }

        [Test]
        public void SnapshotBuffer_RespectsHistoryLimit()
        {
            for (ulong i = 0; i < SnapshotLimit + 5; i++)
            {
                _gameState.HP = (int)i;
                _gameState.Gold = (int)(i * 10);
                _snapshotRepository.CaptureAndSyncSnapshot(i);
            }

            // The oldest 5 entries should be evicted
            Assert.IsFalse(_snapshotRepository.TryGetSnapshot(0, out _));
            Assert.IsFalse(_snapshotRepository.TryGetSnapshot(1, out _));
            Assert.IsTrue(_snapshotRepository.TryGetSnapshot(SnapshotLimit + 4, out var latest));

            var state = _serializer.DeserializeSnapshot(latest.Data);
            Assert.AreEqual(SnapshotLimit + 4, (ulong)state.HP);

            for (ulong i = 0; i < SnapshotLimit + 5; i++)
            {
                if (!_snapshotRepository.TryGetSnapshot(i, out var snapshot))
                    continue;

                var data = _serializer.DeserializeSnapshot(snapshot.Data);
            }
        }

        [Test]
        public async Task RollbackTest()
        {
            _network.Options.BaseLatencyMs = 1;
            _network.Options.LatencyJitterMs = 0;

            var rollbackFinished = new TaskCompletionSource<bool>();

            for (ulong i = 0; i < 50; i++)
            {
                _gameState.HP = (int)i;
                _gameState.Gold = (int)(i * 10);
                _snapshotRepository.CaptureAndSyncSnapshot(i);

                UnityEngine.Debug.Log($"Frame:{i} -> {JsonConvert.SerializeObject(_gameState)}");
            }

            if (_snapshotRepository.TryGetSnapshot(49, out var lastSnapshot))
            {
                var state = _serializer.DeserializeSnapshot(lastSnapshot.Data);
                UnityEngine.Debug.Log($"Last Frame:{lastSnapshot.Tick} -> {JsonConvert.SerializeObject(_gameState)}");
            }

            _network.RequestRollback(50);

            ulong simulatedFrame = 0;

            _rollbackHandler.CheckAndApplyRollback(0,
                onPreparing: () =>
                {
                    UnityEngine.Debug.Log($"Preparing Rollback");
                },
                onStarted: (toFrame, commandsHistory) =>
                {
                    UnityEngine.Debug.Log($"Start Rollback: toFrame:{toFrame}");
                },
                onFinished: () =>
                {
                    UnityEngine.Debug.Log($"Rollback Finished");
                    rollbackFinished.SetResult(true);
                });

            var timeout = DateTime.UtcNow + TimeSpan.FromSeconds(5);
            while (!rollbackFinished.Task.IsCompleted && DateTime.UtcNow < timeout)
            {
                await Task.Delay(20); // simulate frame delay
                simulatedFrame++;
                _rollbackHandler.CheckAndApplyRollback(simulatedFrame,
                    onPreparing: () => { },
                    onStarted: (_, __) => { },
                    onFinished: () => rollbackFinished.TrySetResult(true));
            }

            Assert.IsTrue(rollbackFinished.Task.IsCompleted, "Rollback did not complete in time.");
        }

        private void HandleRollback(ISerializableGameState safeSnapshot, ulong frame)
        {
            UnityEngine.Debug.Log($"Last Frame:{frame} -> {JsonConvert.SerializeObject(safeSnapshot)}");
        }
    }
}
