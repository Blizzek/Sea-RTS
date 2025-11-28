using UnityEngine;
using Unity.Netcode;
using SeaRTS.Network;

namespace SeaRTS.Core
{
    /// <summary>
    /// Main game manager that handles game state and spawning.
    /// </summary>
    public class GameManager : NetworkBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Prefabs")]
        [SerializeField] private GameObject playerPrefab;
        [SerializeField] private GameObject[] unitPrefabs;

        [Header("Spawn Points")]
        [SerializeField] private Transform[] playerSpawnPoints;

        [Header("Game Settings")]
        [SerializeField] private int startingResources = 1000;
        [SerializeField] private int startingUnits = 3;

        public NetworkVariable<GameState> CurrentGameState = new NetworkVariable<GameState>(GameState.WaitingForPlayers);

        public enum GameState
        {
            WaitingForPlayers,
            Starting,
            InProgress,
            Ended
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public override void OnNetworkSpawn()
        {
            base.OnNetworkSpawn();

            if (IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += HandlePlayerConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += HandlePlayerDisconnected;
            }

            Debug.Log("[SeaRTS] GameManager spawned");
        }

        public override void OnNetworkDespawn()
        {
            if (IsServer)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= HandlePlayerConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= HandlePlayerDisconnected;
            }

            base.OnNetworkDespawn();
        }

        private void HandlePlayerConnected(ulong clientId)
        {
            Debug.Log($"[SeaRTS] Player connected: {clientId}");
            SpawnPlayerUnits(clientId);
        }

        private void HandlePlayerDisconnected(ulong clientId)
        {
            Debug.Log($"[SeaRTS] Player disconnected: {clientId}");
        }

        private void SpawnPlayerUnits(ulong clientId)
        {
            if (!IsServer || unitPrefabs == null || unitPrefabs.Length == 0) return;

            // Get spawn point for this player
            int spawnIndex = (int)(clientId % (ulong)Mathf.Max(1, playerSpawnPoints?.Length ?? 1));
            Vector3 spawnPosition = playerSpawnPoints != null && playerSpawnPoints.Length > spawnIndex
                ? playerSpawnPoints[spawnIndex].position
                : Vector3.zero;

            // Spawn starting units
            for (int i = 0; i < startingUnits; i++)
            {
                Vector3 offset = new Vector3(i * 3f, 0, 0);
                SpawnUnit(0, spawnPosition + offset, clientId);
            }

            Debug.Log($"[SeaRTS] Spawned {startingUnits} units for player {clientId}");
        }

        /// <summary>
        /// Spawns a unit for a specific player.
        /// </summary>
        public void SpawnUnit(int unitTypeIndex, Vector3 position, ulong ownerClientId)
        {
            if (!IsServer) return;

            if (unitPrefabs == null || unitTypeIndex < 0 || unitTypeIndex >= unitPrefabs.Length)
            {
                Debug.LogError("[SeaRTS] Invalid unit type index");
                return;
            }

            GameObject unitPrefab = unitPrefabs[unitTypeIndex];
            if (unitPrefab == null) return;

            GameObject unitInstance = Instantiate(unitPrefab, position, Quaternion.identity);
            NetworkObject networkObject = unitInstance.GetComponent<NetworkObject>();

            if (networkObject != null)
            {
                networkObject.SpawnWithOwnership(ownerClientId);
            }
        }

        /// <summary>
        /// Starts the game (Server only).
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void StartGameServerRpc()
        {
            if (!IsServer) return;

            CurrentGameState.Value = GameState.InProgress;
            Debug.Log("[SeaRTS] Game started!");
        }

        /// <summary>
        /// Ends the game (Server only).
        /// </summary>
        [ServerRpc(RequireOwnership = false)]
        public void EndGameServerRpc(ulong winnerClientId)
        {
            if (!IsServer) return;

            CurrentGameState.Value = GameState.Ended;
            NotifyGameEndClientRpc(winnerClientId);
        }

        [ClientRpc]
        private void NotifyGameEndClientRpc(ulong winnerClientId)
        {
            Debug.Log($"[SeaRTS] Game ended! Winner: Player {winnerClientId}");
        }

        /// <summary>
        /// Checks if all players are ready to start.
        /// </summary>
        public bool AreAllPlayersReady()
        {
            foreach (var player in FindObjectsOfType<PlayerNetwork>())
            {
                if (!player.IsReady.Value)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Gets the current number of players.
        /// </summary>
        public int GetPlayerCount()
        {
            return NetworkManager.Singleton?.ConnectedClients?.Count ?? 0;
        }
    }
}
