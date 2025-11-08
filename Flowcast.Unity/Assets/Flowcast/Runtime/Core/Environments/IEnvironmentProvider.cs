// Runtime/Core/Environments/IEnvironmentProvider.cs
using System;

namespace Flowcast.Core.Environments
{
    public interface IEnvironmentProvider
    {
        Environment Current { get; }
        event Action<Environment> Changed;

        /// <summary>Switch the active environment for this process.</summary>
        void Set(Environment env);

        /// <summary>Active environment id (if any) persisted in PlayerPrefs.</summary>
        string PersistedEnvironmentId { get; }

        /// <summary>Clear persisted selection so defaults apply next run.</summary>
        void ClearPersistedSelection();
    }
}
