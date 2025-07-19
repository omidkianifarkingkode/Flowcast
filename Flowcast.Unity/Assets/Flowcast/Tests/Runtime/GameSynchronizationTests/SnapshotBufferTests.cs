using Flowcast.Commons;
using Flowcast.Logging;
using Flowcast.Network;
using Flowcast.Serialization;
using Flowcast.Synchronization;
using NUnit.Framework;
using System.IO;

namespace Flowcast.Tests.Runtime.GameSynchronizationTests
{
    public class SampleGameState : IBinarySerializableGameState
    {
        public int HP { get; set; }
        public int Gold { get; set; }

        public void ReadFrom(BinaryReader reader)
        {
            HP = reader.ReadInt32();
            Gold = reader.ReadInt32();
        }

        public void WriteTo(BinaryWriter writer)
        {
            writer.Write(HP);
            writer.Write(Gold);
        }
    }

    [TestFixture]
    public class SnapshotBufferTests
    {
        private SampleGameState _gameState;
        private SnapshotRepository _snapshotRepository;
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
                SnapshotHistoryLimit = SnapshotLimit
            };
            var hasher = new XorHasher();
            var logger = new UnityLogger();

            _snapshotRepository = new SnapshotRepository(_serializer, hasher, _network, _options, logger);
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

    }
}
