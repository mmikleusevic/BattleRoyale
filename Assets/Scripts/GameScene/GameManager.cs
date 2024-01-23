using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GameManager : StateMachine
{
    public static GameManager Instance { get; private set; }

    public event EventHandler OnToggleLocalGamePause;
    public event EventHandler OnMultiplayerGamePaused;
    public event EventHandler OnMultiplayerGameUnpaused;
    public event EventHandler<GameState> OnGameStateChanged;

    [SerializeField] private Transform playerPrefab;
    [SerializeField] private List<Vector3> spawnPositionList;

    private bool autoCheckGamePauseState;

    private List<Player> players;
    private Dictionary<ulong, bool> playerReadyDictonary;
    private Dictionary<ulong, bool> playerPausedDictionary;
    private NetworkVariable<bool> isGamePaused;
    private NetworkVariable<GameState> gameState;

    private void Awake()
    {
        Instance = this;

        players = new List<Player>();
        playerReadyDictonary = new Dictionary<ulong, bool>();
        playerPausedDictionary = new Dictionary<ulong, bool>();
        isGamePaused = new NetworkVariable<bool>(false);
        gameState = new NetworkVariable<GameState>(GameState.WaitingToStart);
    }

    public override void OnNetworkSpawn()
    {
        gameState.OnValueChanged += GameState_OnValueChanged;
        isGamePaused.OnValueChanged += IsGamePaused_OnValueChanged;
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
    }

    public override void OnNetworkDespawn()
    {
        gameState.OnValueChanged -= GameState_OnValueChanged;
        isGamePaused.OnValueChanged -= IsGamePaused_OnValueChanged;

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= NetworkManager_OnClientDisconnectCallback;
        }
    }

    private void Start()
    {
        SetPlayerReadyServerRpc();
    }

    private void LateUpdate()
    {
        if (autoCheckGamePauseState)
        {
            autoCheckGamePauseState = false;
            CheckGamePauseState();
        }
    }

    private void GameState_OnValueChanged(GameState previousValue, GameState newValue)
    {
        OnGameStateChanged?.Invoke(this, newValue);
    }

    private void IsGamePaused_OnValueChanged(bool previousValue, bool newValue)
    {
        if (isGamePaused.Value)
        {
            Time.timeScale = 0f;
            OnMultiplayerGamePaused?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            Time.timeScale = 1f;
            OnMultiplayerGameUnpaused?.Invoke(this, EventArgs.Empty);
        }
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong obj)
    {
        if (obj == NetworkManager.ServerClientId && NetworkManager.IsConnectedClient)
        {
            GameLobby.Instance.LeaveLobbyOrDelete();
            LevelManager.Instance.LoadScene(Scene.MainMenuScene);
        }
    }

    [ServerRpc]
    private void StartGameServerRpc()
    {
        SpawnPlayers();

        SetPlayerToPlayersListClientRpc();

        gameState.Value = GameState.GamePlaying;
    }

    public void TogglePauseGame()
    {
        TogglePauseGameServerRpc();

        OnToggleLocalGamePause?.Invoke(this, EventArgs.Empty);
    }

    [ServerRpc(RequireOwnership = false)]
    private void TogglePauseGameServerRpc(ServerRpcParams serverRpcParams = default)
    {
        if (!playerPausedDictionary.ContainsKey(serverRpcParams.Receive.SenderClientId))
        {
            playerPausedDictionary[serverRpcParams.Receive.SenderClientId] = true;
        }
        else
        {
            playerPausedDictionary[serverRpcParams.Receive.SenderClientId] = !playerPausedDictionary[serverRpcParams.Receive.SenderClientId];
        }

        CheckGamePauseState();
    }

    private void CheckGamePauseState()
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (playerPausedDictionary.ContainsKey(clientId) && playerPausedDictionary[clientId])
            {
                isGamePaused.Value = true;
                return;
            }
        }

        isGamePaused.Value = false;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        playerReadyDictonary[serverRpcParams.Receive.SenderClientId] = true;

        bool allClientsReady = true;
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playerReadyDictonary.ContainsKey(clientId) || !playerReadyDictonary[clientId])
            {
                allClientsReady = false;
                break;
            }
        }

        if (allClientsReady)
        {
            StartGameServerRpc();
        }
    }

    private void SpawnPlayers()
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            int playerIndex = GameMultiplayer.Instance.GetPlayerDataIndexFromClientId(clientId);
            Vector3 position = spawnPositionList[playerIndex];

            Transform playerTransform = Instantiate(playerPrefab, position, playerPrefab.rotation, null);
            NetworkObject playerNetworkObject = playerTransform.GetComponent<NetworkObject>();
            playerNetworkObject.SpawnAsPlayerObject(clientId, true);
            GameMultiplayer.Instance.SetNameClientRpc(playerTransform.gameObject, "Player" + playerIndex);
        }
    }

    [ClientRpc]
    private void SetPlayerToPlayersListClientRpc()
    {
        Player[] players = FindObjectsByType<Player>(FindObjectsSortMode.InstanceID);

        foreach (Player player in players)
        {
            this.players.Add(player);
        }
    }
}
