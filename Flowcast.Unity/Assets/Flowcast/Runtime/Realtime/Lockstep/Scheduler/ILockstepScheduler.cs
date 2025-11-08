using System;
using System.Collections.Generic;
using UnityEngine;

namespace Flowcast.Lockstep
{
    public interface ILockstepScheduler
    {
        void Schedule(Action action, int delayMilliseconds); // delay-based
        void Schedule(ulong targetFrame, Action action);     // absolute frame-based
    }

    public class LockstepScheduler : ILockstepScheduler
    {
        private readonly ILockstepSettings _settings;
        private readonly SortedDictionary<ulong, List<Action>> _scheduledActions = new();
        private ulong _currentFrame;

        public LockstepScheduler(ILockstepSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }

        /// <summary>
        /// Schedules an action to run after a delay in milliseconds, based on simulation time.
        /// </summary>
        public void Schedule(Action action, int delayMilliseconds)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            int delayFrames = Mathf.CeilToInt(delayMilliseconds * _settings.GameFramesPerSecond / 1000f);
            ulong targetFrame = _currentFrame + (ulong)delayFrames;

            Schedule(targetFrame, action);
        }

        /// <summary>
        /// Schedules an action to run at a specific simulation frame.
        /// </summary>
        public void Schedule(ulong targetFrame, Action action)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            if (!_scheduledActions.TryGetValue(targetFrame, out var list))
            {
                list = new List<Action>();
                _scheduledActions[targetFrame] = list;
            }

            list.Add(action);
        }

        /// <summary>
        /// Called by the engine each simulation step to execute any scheduled actions.
        /// </summary>
        public void UpdateFrame(ulong currentFrame)
        {
            _currentFrame = currentFrame;

            if (_scheduledActions.TryGetValue(currentFrame, out var actions))
            {
                foreach (var action in actions)
                {
                    try
                    {
                        action.Invoke();
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Exception in scheduled action: {ex}");
                    }
                }

                _scheduledActions.Remove(currentFrame);
            }
        }

        public void ResetToFrame(ulong frame)
        {
            _currentFrame = frame;

            // Remove all scheduled actions at or after the reset frame
            var keysToRemove = new List<ulong>();

            foreach (var kvp in _scheduledActions)
            {
                if (kvp.Key >= frame)
                    keysToRemove.Add(kvp.Key);
            }

            foreach (var key in keysToRemove)
            {
                _scheduledActions.Remove(key);
            }
        }

    }
}
