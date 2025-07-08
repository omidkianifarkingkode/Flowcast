using System;
using UnityEngine;

namespace Flowcast.Network
{
    [Serializable]
    public class DummyNetworkServerOptions
    {
        /// <summary>
        /// Base round-trip latency in milliseconds (client → server → client).
        /// </summary>
        public int BaseLatencyMs = 80;

        /// <summary>
        /// Maximum ± jitter (random offset) applied to latency, in milliseconds.
        /// </summary>
        public int LatencyJitterMs = 20;

        /// <summary>
        /// Simulated packet loss percentage (0–100).
        /// </summary>
        [Range(0, 100)]
        public int PacketLossPercent = 0;

        /// <summary>
        /// If true, automatically echo inputs back to simulate server receipt.
        /// </summary>
        public bool EchoInputs = true;

        /// <summary>
        /// If true, randomly trigger rollback events during sync.
        /// </summary>
        public bool SimulateRandomRollback = false;

        /// <summary>
        /// Chance (0–100%) of triggering a rollback per sync check.
        /// </summary>
        [Range(0, 100)]
        public int RollbackChancePercent = 5;

        /// <summary>
        /// Computes simulated latency with jitter.
        /// </summary>
        public TimeSpan GetRandomLatency()
        {
            int jitter = UnityEngine.Random.Range(-LatencyJitterMs, LatencyJitterMs);
            int total = Math.Max(0, BaseLatencyMs + jitter);
            return TimeSpan.FromMilliseconds(total / 2); // half-latency per direction
        }

        /// <summary>
        /// Simulates packet loss.
        /// </summary>
        public bool ShouldDropPacket()
        {
            return UnityEngine.Random.Range(0, 100) < PacketLossPercent;
        }

        /// <summary>
        /// Simulates a random rollback trigger.
        /// </summary>
        public bool ShouldTriggerRollback()
        {
            return SimulateRandomRollback && UnityEngine.Random.Range(0, 100) < RollbackChancePercent;
        }
    }
}
