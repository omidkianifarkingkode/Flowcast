// Runtime/Core/Environments/FlowcastClientSettings.cs
using UnityEngine;

namespace Flowcast.Core.Environments
{
    [CreateAssetMenu(
        fileName = "FlowcastClientSettings",
        menuName = "Flowcast/Core/Client Settings",
        order = 2)]
    public sealed class FlowcastClientSettings : ScriptableObject
    {
        [Tooltip("Set of available environments (Dev, Test, Prod...).")]
        [SerializeField] private EnvironmentSet environmentSet;

        [Tooltip("If set, this environment is used unless an override is present.")]
        [SerializeField] private Environment preferredEnvironment;

        [Tooltip("Persist last selected environment across sessions (PlayerPrefs).")]
        [SerializeField] private bool persistSelection = true;

        [Tooltip("Optional PlayerPrefs key override for the active environment id.")]
        [SerializeField] private string prefsKey = "flowcast.active_env";

        public EnvironmentSet EnvironmentSet => environmentSet;
        public Environment PreferredEnvironment => preferredEnvironment;
        public bool PersistSelection => persistSelection;
        public string PrefsKey => string.IsNullOrWhiteSpace(prefsKey) ? "flowcast.active_env" : prefsKey;

        /// <summary>
        /// Load from Resources at path: Resources/Flowcast/ClientSettings.asset
        /// </summary>
        public static FlowcastClientSettings LoadFromResources()
            => Resources.Load<FlowcastClientSettings>("Flowcast/ClientSettings");
    }
}
