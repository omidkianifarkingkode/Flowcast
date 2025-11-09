using System.Threading.Tasks;
using Flowcast.Core.Common;
using Flowcast.Rest.Bootstrap;
using Flowcast.Rest.Client;
using UnityEngine;

namespace Flowcast.Samples
{
    /// <summary>
    /// Minimal MonoBehaviour that demonstrates how to call Flowcast.Rest once the
    /// FlowcastRestBootstrapper has been configured in the scene.
    /// </summary>
    public sealed class FlowcastRestUsageSample : MonoBehaviour
    {
        [Header("Sample Input")]
        [SerializeField] private string playerId = "demo";

        private async void Start()
        {
            if (FlowcastRest.Instance == null)
            {
                Debug.LogWarning("[Flowcast] FlowcastRest.Instance is not configured. Add FlowcastRestBootstrapper to the scene.");
                return;
            }

            await FetchProfileAsync();
            await UpdateDisplayNameAsync();
        }

        private async Task FetchProfileAsync()
        {
            Result<PlayerProfile> result = await FlowcastRest.GetAsync<PlayerProfile>($"/v1/players/{playerId}");
            if (result.IsSuccess)
            {
                Debug.Log($"[Flowcast] Player '{result.Value.displayName}' loaded.");
            }
            else
            {
                Debug.LogWarning($"[Flowcast] Failed to load player: {result.Error}");
            }
        }

        private async Task UpdateDisplayNameAsync()
        {
            var payload = new UpdateDisplayNameRequest { displayName = $"Player {Random.Range(100, 999)}" };

            Result<PlayerProfile> result = await FlowcastRest
                .Send("PATCH", $"/v1/players/{playerId}")
                .RequireAuth()
                .WithIdempotencyKey()
                .WithBody(payload)
                .AsResultAsync<PlayerProfile>();

            if (result.IsSuccess)
            {
                Debug.Log($"[Flowcast] Display name updated to '{result.Value.displayName}'.");
            }
            else
            {
                Debug.LogWarning($"[Flowcast] Failed to update player: {result.Error}");
            }
        }

        private sealed class PlayerProfile
        {
            public string id;
            public string displayName;
        }

        private sealed class UpdateDisplayNameRequest
        {
            public string displayName;
        }
    }
}
