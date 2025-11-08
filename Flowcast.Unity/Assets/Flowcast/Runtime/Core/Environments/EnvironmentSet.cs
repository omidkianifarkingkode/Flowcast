// Runtime/Core/Environments/EnvironmentSet.cs
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Flowcast.Core.Environments
{
    [CreateAssetMenu(
        fileName = "EnvironmentSet",
        menuName = "Flowcast/Core/Environment Set",
        order = 1)]
    public sealed class EnvironmentSet : ScriptableObject
    {
        [SerializeField] private List<Environment> environments = new();
        [SerializeField] private Environment defaultEnvironment;

        public IReadOnlyList<Environment> Environments => environments;
        public Environment DefaultEnvironmentFallback =>
            defaultEnvironment != null ? defaultEnvironment :
            (environments.Count > 0 ? environments[0] : null);

        public bool TryGetById(string id, out Environment env)
        {
            if (!string.IsNullOrEmpty(id))
            {
                foreach (var e in environments)
                {
                    if (e != null && string.Equals(e.Id, id, StringComparison.OrdinalIgnoreCase))
                    {
                        env = e;
                        return true;
                    }
                }
            }
            env = null;
            return false;
        }
    }
}
