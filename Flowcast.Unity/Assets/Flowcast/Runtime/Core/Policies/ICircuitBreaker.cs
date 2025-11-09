// Runtime/Core/Policies/ICircuitBreaker.cs
using System;

namespace Flowcast.Core.Policies
{
    public interface ICircuitBreaker
    {
        bool IsOpen { get; }
        bool AllowRequest();               // false => short-circuit
        void OnSuccess();                  // reset counters, close if half-open
        void OnFailure();                  // increment failures, maybe open
    }

    /// KISS breaker:
    /// - Opens after N consecutive failures.
    /// - Stays open for OpenDuration.
    /// - After that, allows 1 trial (half-open). Success closes; failure opens again.
    public sealed class SimpleCircuitBreaker : ICircuitBreaker
    {
        private readonly int _failureThreshold;
        private readonly TimeSpan _openDuration;

        private int _consecutiveFailures;
        private DateTimeOffset _openedAtUtc = DateTimeOffset.MinValue;
        private bool _trialAllowed = true;

        public SimpleCircuitBreaker(int failureThreshold = 5, int openMs = 5000)
        {
            _failureThreshold = Math.Max(1, failureThreshold);
            _openDuration = TimeSpan.FromMilliseconds(Math.Max(100, openMs));
        }

        public bool IsOpen => _openedAtUtc > DateTimeOffset.UtcNow - _openDuration;

        public bool AllowRequest()
        {
            // Closed
            if (!IsOpen) return true;

            // Half-open: allow one trial after open duration
            if (DateTimeOffset.UtcNow - _openedAtUtc >= _openDuration)
            {
                if (_trialAllowed)
                {
                    _trialAllowed = false;
                    return true;
                }
            }
            return false;
        }

        public void OnSuccess()
        {
            _consecutiveFailures = 0;
            _openedAtUtc = DateTimeOffset.MinValue;
            _trialAllowed = true;
        }

        public void OnFailure()
        {
            _consecutiveFailures++;
            if (_consecutiveFailures >= _failureThreshold)
            {
                _openedAtUtc = DateTimeOffset.UtcNow;
                _trialAllowed = true;
            }
        }
    }
}
