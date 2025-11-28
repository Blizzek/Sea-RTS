using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SeaRTS.Network;

namespace SeaRTS.UI
{
    /// <summary>
    /// Main menu UI for hosting and joining multiplayer games.
    /// </summary>
    public class MainMenuUI : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject mainMenuPanel;
        [SerializeField] private GameObject hostPanel;
        [SerializeField] private GameObject joinPanel;
        [SerializeField] private GameObject lobbyPanel;

        [Header("Main Menu Buttons")]
        [SerializeField] private Button hostButton;
        [SerializeField] private Button joinButton;
        [SerializeField] private Button quitButton;

        [Header("Host Panel")]
        [SerializeField] private TMP_InputField hostPlayerNameInput;
        [SerializeField] private Button startHostButton;
        [SerializeField] private Button hostBackButton;

        [Header("Join Panel")]
        [SerializeField] private TMP_InputField joinPlayerNameInput;
        [SerializeField] private TMP_InputField ipAddressInput;
        [SerializeField] private TMP_InputField portInput;
        [SerializeField] private Button connectButton;
        [SerializeField] private Button joinBackButton;

        [Header("Lobby Panel")]
        [SerializeField] private TextMeshProUGUI lobbyStatusText;
        [SerializeField] private TextMeshProUGUI playerListText;
        [SerializeField] private Button startGameButton;
        [SerializeField] private Button disconnectButton;

        private GameNetworkManager networkManager;

        private void Start()
        {
            networkManager = GameNetworkManager.Instance;
            SetupUI();
            ShowMainMenu();
        }

        private void OnEnable()
        {
            if (networkManager != null)
            {
                networkManager.OnHostStarted += HandleHostStarted;
                networkManager.OnClientConnected += HandleClientConnected;
                networkManager.OnClientDisconnected += HandleClientDisconnected;
                networkManager.OnConnectionFailed += HandleConnectionFailed;
            }
        }

        private void OnDisable()
        {
            if (networkManager != null)
            {
                networkManager.OnHostStarted -= HandleHostStarted;
                networkManager.OnClientConnected -= HandleClientConnected;
                networkManager.OnClientDisconnected -= HandleClientDisconnected;
                networkManager.OnConnectionFailed -= HandleConnectionFailed;
            }
        }

        private void SetupUI()
        {
            // Main Menu
            if (hostButton != null) hostButton.onClick.AddListener(ShowHostPanel);
            if (joinButton != null) joinButton.onClick.AddListener(ShowJoinPanel);
            if (quitButton != null) quitButton.onClick.AddListener(QuitGame);

            // Host Panel
            if (startHostButton != null) startHostButton.onClick.AddListener(StartHost);
            if (hostBackButton != null) hostBackButton.onClick.AddListener(ShowMainMenu);

            // Join Panel
            if (connectButton != null) connectButton.onClick.AddListener(JoinGame);
            if (joinBackButton != null) joinBackButton.onClick.AddListener(ShowMainMenu);

            // Lobby Panel
            if (startGameButton != null) startGameButton.onClick.AddListener(StartGame);
            if (disconnectButton != null) disconnectButton.onClick.AddListener(Disconnect);

            // Set default values
            if (ipAddressInput != null) ipAddressInput.text = "127.0.0.1";
            if (portInput != null) portInput.text = "7777";
        }

        private void ShowMainMenu()
        {
            SetActivePanel(mainMenuPanel);
        }

        private void ShowHostPanel()
        {
            SetActivePanel(hostPanel);
        }

        private void ShowJoinPanel()
        {
            SetActivePanel(joinPanel);
        }

        private void ShowLobbyPanel()
        {
            SetActivePanel(lobbyPanel);
            UpdateLobbyUI();
        }

        private void SetActivePanel(GameObject panel)
        {
            if (mainMenuPanel != null) mainMenuPanel.SetActive(panel == mainMenuPanel);
            if (hostPanel != null) hostPanel.SetActive(panel == hostPanel);
            if (joinPanel != null) joinPanel.SetActive(panel == joinPanel);
            if (lobbyPanel != null) lobbyPanel.SetActive(panel == lobbyPanel);
        }

        private void StartHost()
        {
            if (networkManager == null)
            {
                Debug.LogError("[SeaRTS] NetworkManager not found!");
                return;
            }

            networkManager.StartHost();
        }

        private void JoinGame()
        {
            if (networkManager == null)
            {
                Debug.LogError("[SeaRTS] NetworkManager not found!");
                return;
            }

            string ip = ipAddressInput != null ? ipAddressInput.text : "127.0.0.1";
            ushort port = 7777;

            if (portInput != null && ushort.TryParse(portInput.text, out ushort parsedPort))
            {
                port = parsedPort;
            }

            networkManager.SetIPAddress(ip);
            networkManager.SetPort(port);
            networkManager.JoinGame(ip, port);
        }

        private void StartGame()
        {
            // Load the game scene
            Unity.Netcode.NetworkManager.Singleton.SceneManager.LoadScene("GameScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }

        private void Disconnect()
        {
            if (networkManager != null)
            {
                networkManager.Disconnect();
            }
            ShowMainMenu();
        }

        private void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void HandleHostStarted()
        {
            ShowLobbyPanel();
            UpdateLobbyStatus("Hosting game...\nWaiting for players to join.");

            if (startGameButton != null)
            {
                startGameButton.gameObject.SetActive(true);
            }
        }

        private void HandleClientConnected()
        {
            ShowLobbyPanel();
            UpdateLobbyStatus("Connected to host!\nWaiting for game to start.");

            if (startGameButton != null)
            {
                startGameButton.gameObject.SetActive(false);
            }
        }

        private void HandleClientDisconnected()
        {
            ShowMainMenu();
        }

        private void HandleConnectionFailed(string error)
        {
            Debug.LogError($"[SeaRTS] Connection failed: {error}");
            ShowMainMenu();
        }

        private void UpdateLobbyUI()
        {
            if (networkManager == null) return;

            int playerCount = networkManager.GetConnectedPlayerCount();
            int maxPlayers = networkManager.GetMaxPlayers();

            if (playerListText != null)
            {
                playerListText.text = $"Players: {playerCount}/{maxPlayers}";
            }
        }

        private void UpdateLobbyStatus(string status)
        {
            if (lobbyStatusText != null)
            {
                lobbyStatusText.text = status;
            }
        }

        private void Update()
        {
            if (lobbyPanel != null && lobbyPanel.activeSelf)
            {
                UpdateLobbyUI();
            }
        }
    }
}
