using Flowcast.Commands;
using System;
using System.Collections.Generic;

namespace Flowcast.Rollback
{
    public interface IRollbackHandler
    {
        bool IsInRecovery { get; }
        RollbackState State { get; }

        event Action<ulong> OnRollbackPrepared;
        event Action<ulong> OnRollbackStarted;
        event Action OnRollbackFinished;

        void CheckAndApplyRollback(ulong frame, Action onPreparing, Action<ulong, IReadOnlyCollection<ICommand>> onStarted, Action onFinished);
    }
}
