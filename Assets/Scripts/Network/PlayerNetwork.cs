using UnityEngine;
using Unity.Netcode;
using Unity.Collections;

namespace SeaRTS.Network
{
    /// <summary>
    /// Represents a networked player in the Sea-RTS game.
    /// Handles player-specific network data and synchronization.
    /// </summary>
    public class PlayerNetwork : NetworkBehaviour
    {
        [Header("Player Info")]
        public NetworkVariable<FixedString64Bytes> PlayerName = new NetworkVariable<FixedString64Bytes>(
            default,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );

        public NetworkVariable<int> TeamId = new NetworkVariable<int>(
            0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        public NetworkVariable<Color> PlayerColor = new NetworkVariable<Color>(
            Color.blue,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        public NetworkVariable<int> Resources = new NetworkVariable<int>(
            1000,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
        );

        public NetworkVariable<bool> IsReady = new NetworkVariable<bool>(
            false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner
        );

        public event System.Action<int> OnResourcesChanged;
        public event System.Action OnPlayerReady;

        private static readonly Color[] TeamColors = new Color[]
        {
            Color.blue,
            Color.red,
            Color.green,
            Color.yellow
        };

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsOwner)
            {
                PlayerName.Value = new FixedString64Bytes($"Player_{OwnerClientId}");
            }

            if (IsServer)
            {
                AssignTeam();
            }

            Resources.OnValueChanged += HandleResourcesChanged;
            IsReady.OnValueChanged += HandleReadyChanged;

            Debug.Log($"[SeaRTS] Player spawned: {PlayerName.Value} (Client: {OwnerClientId})");
        }

        public override void OnNetworkDespawn()
        {
            Resources.OnValueChanged -= HandleResourcesChanged;
            IsReady.OnValueChanged -= HandleReadyChanged;

            base.OnNetworkDespawn();
        }

        private void AssignTeam()
        {
            int teamId = (int)(OwnerClientId % 4);
            TeamId.Value = teamId;
            PlayerColor.Value = TeamColors[teamId];
        }

        private void HandleResourcesChanged(int previousValue, int newValue)
        {
            OnResourcesChanged?.Invoke(newValue);
        }

        private void HandleReadyChanged(bool previousValue, bool newValue)
        {
            if (newValue)
            {
                OnPlayerReady?.Invoke();
            }
        }

        /// <summary>
        /// Adds resources to the player (Server only).
        /// </summary>
        [ServerRpc]
        public void AddResourcesServerRpc(int amount)
        {
            Resources.Value += amount;
        }

        /// <summary>
        /// Spends resources if the player has enough (Server only).
        /// </summary>
        [ServerRpc]
        public void SpendResourcesServerRpc(int amount, ServerRpcParams rpcParams = default)
        {
            if (Resources.Value >= amount)
            {
                Resources.Value -= amount;
            }
        }

        /// <summary>
        /// Checks if the player can afford a cost.
        /// </summary>
        public bool CanAfford(int cost)
        {
            return Resources.Value >= cost;
        }

        /// <summary>
        /// Sets the player's ready state.
        /// </summary>
        public void SetReady(bool ready)
        {
            if (IsOwner)
            {
                IsReady.Value = ready;
            }
        }

        /// <summary>
        /// Sets the player's name.
        /// </summary>
        public void SetPlayerName(string name)
        {
            if (IsOwner)
            {
                PlayerName.Value = new FixedString64Bytes(name);
            }
        }
    }
}
