using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;

namespace SeaRTS.Network
{
    /// <summary>
    /// Manages network connections and multiplayer sessions for the Sea-RTS game.
    /// Provides functionality for hosting, joining, and managing multiplayer games.
    /// </summary>
    public class GameNetworkManager : MonoBehaviour
    {
        public static GameNetworkManager Instance { get; private set; }

        [Header("Network Settings")]
        [SerializeField] private string ipAddress = "127.0.0.1";
        [SerializeField] private ushort port = 7777;
        [SerializeField] private int maxPlayers = 4;

        [Header("Connection Status")]
        public bool IsHost { get; private set; }
        public bool IsClient { get; private set; }
        public bool IsConnected { get; private set; }

        public event System.Action OnHostStarted;
        public event System.Action OnClientConnected;
        public event System.Action OnClientDisconnected;
        public event System.Action<string> OnConnectionFailed;

        private NetworkManager networkManager;
        private UnityTransport transport;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            SetupNetworkManager();
        }

        private void SetupNetworkManager()
        {
            networkManager = GetComponent<NetworkManager>();
            if (networkManager == null)
            {
                networkManager = gameObject.AddComponent<NetworkManager>();
            }

            transport = GetComponent<UnityTransport>();
            if (transport == null)
            {
                transport = gameObject.AddComponent<UnityTransport>();
            }

            networkManager.NetworkConfig = new NetworkConfig();
        }

        private void OnEnable()
        {
            if (networkManager != null)
            {
                networkManager.OnClientConnectedCallback += HandleClientConnected;
                networkManager.OnClientDisconnectCallback += HandleClientDisconnected;
            }
        }

        private void OnDisable()
        {
            if (networkManager != null)
            {
                networkManager.OnClientConnectedCallback -= HandleClientConnected;
                networkManager.OnClientDisconnectCallback -= HandleClientDisconnected;
            }
        }

        /// <summary>
        /// Starts hosting a game session.
        /// </summary>
        public void StartHost()
        {
            SetupTransport(ipAddress, port);

            if (networkManager.StartHost())
            {
                IsHost = true;
                IsConnected = true;
                Debug.Log($"[SeaRTS] Host started on {ipAddress}:{port}");
                OnHostStarted?.Invoke();
            }
            else
            {
                Debug.LogError("[SeaRTS] Failed to start host");
                OnConnectionFailed?.Invoke("Failed to start host");
            }
        }

        /// <summary>
        /// Joins an existing game session as a client.
        /// </summary>
        /// <param name="hostIP">IP address of the host</param>
        /// <param name="hostPort">Port of the host</param>
        public void JoinGame(string hostIP, ushort hostPort)
        {
            SetupTransport(hostIP, hostPort);

            if (networkManager.StartClient())
            {
                IsClient = true;
                Debug.Log($"[SeaRTS] Connecting to {hostIP}:{hostPort}");
            }
            else
            {
                Debug.LogError("[SeaRTS] Failed to start client");
                OnConnectionFailed?.Invoke("Failed to connect to host");
            }
        }

        /// <summary>
        /// Joins a game using the default IP and port settings.
        /// </summary>
        public void JoinGame()
        {
            JoinGame(ipAddress, port);
        }

        /// <summary>
        /// Disconnects from the current game session.
        /// </summary>
        public void Disconnect()
        {
            if (networkManager.IsHost)
            {
                networkManager.Shutdown();
            }
            else if (networkManager.IsClient)
            {
                networkManager.Shutdown();
            }

            IsHost = false;
            IsClient = false;
            IsConnected = false;

            Debug.Log("[SeaRTS] Disconnected from game");
        }

        private void SetupTransport(string ip, ushort connectionPort)
        {
            if (transport != null)
            {
                transport.ConnectionData.Address = ip;
                transport.ConnectionData.Port = connectionPort;
            }
        }

        private void HandleClientConnected(ulong clientId)
        {
            IsConnected = true;
            Debug.Log($"[SeaRTS] Client connected: {clientId}");
            OnClientConnected?.Invoke();
        }

        private void HandleClientDisconnected(ulong clientId)
        {
            if (clientId == networkManager.LocalClientId)
            {
                IsConnected = false;
                IsClient = false;
                Debug.Log("[SeaRTS] Disconnected from server");
                OnClientDisconnected?.Invoke();
            }
            else
            {
                Debug.Log($"[SeaRTS] Client disconnected: {clientId}");
            }
        }

        /// <summary>
        /// Sets the IP address for hosting/joining.
        /// </summary>
        public void SetIPAddress(string ip)
        {
            ipAddress = ip;
        }

        /// <summary>
        /// Sets the port for hosting/joining.
        /// </summary>
        public void SetPort(ushort newPort)
        {
            port = newPort;
        }

        /// <summary>
        /// Gets the current number of connected players.
        /// </summary>
        public int GetConnectedPlayerCount()
        {
            if (networkManager != null && networkManager.IsServer)
            {
                return networkManager.ConnectedClients.Count;
            }
            return 0;
        }

        /// <summary>
        /// Gets the maximum number of players allowed.
        /// </summary>
        public int GetMaxPlayers()
        {
            return maxPlayers;
        }
    }
}
