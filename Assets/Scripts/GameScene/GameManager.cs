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
    public event EventHandler OnGameStateChanged;

    [SerializeField] private Transform playerPrefab;
    [SerializeField] private List<Vector3> spawnPositionList = new List<Vector3>();

    private bool autoCheckGamePauseState;
    private bool rollForInitiative = true;

    private List<Player> players;
    private Dictionary<ulong, bool> playerReadyDictonary;
    private Dictionary<ulong, bool> playerPausedDictionary;
    private Dictionary<ulong, int> playerInitiativeOrder;
    private NetworkVariable<bool> isGamePaused;
    private NetworkVariable<GameState> gameState;

    private void Awake()
    {
        Instance = this;

        players = new List<Player>();
        playerReadyDictonary = new Dictionary<ulong, bool>();
        playerPausedDictionary = new Dictionary<ulong, bool>();
        playerInitiativeOrder = new Dictionary<ulong, int>();
        isGamePaused = new NetworkVariable<bool>(false);
        gameState = new NetworkVariable<GameState>(GameState.WaitingToStart);
    }

    public override void OnNetworkSpawn()
    {
        gameState.OnValueChanged += GameState_OnValueChanged;
        isGamePaused.OnValueChanged += IsGamePaused_OnValueChanged;

        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        }
    }

    public override void OnNetworkDespawn()
    {
        gameState.OnValueChanged -= GameState_OnValueChanged;
        isGamePaused.OnValueChanged -= IsGamePaused_OnValueChanged;
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
        OnGameStateChanged?.Invoke(this, EventArgs.Empty);
    }

    [ServerRpc]
    private void StartGameServerRpc()
    {
        SetPlayerToPlayersListClientRpc();

        gameState.Value = GameState.GamePlaying;
    }

    private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
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

    [ClientRpc]
    private void SetPlayerToPlayersListClientRpc(ClientRpcParams clientRpcParams = default)
    {
        Player[] players = FindObjectsByType<Player>(FindObjectsSortMode.InstanceID);

        foreach (Player player in players)
        {
            this.players.Add(player);
        }
    }

    public void SetRollResults(int result)
    {
        if (rollForInitiative)
        {
            SetInitiativeResultServerRpc(result);
        }
        //else
        //{
        //    SetBattleResultServerRpc(result);
        //}
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetInitiativeResultServerRpc(int result, ServerRpcParams serverRpcParams = default)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;

        Debug.Log("clientId: " + clientId);

        SetInitiativeResultClientRpc(result, clientId);
    }

    [ClientRpc]
    private void SetInitiativeResultClientRpc(int result, ulong clientId)
    {
        if (playerInitiativeOrder.ContainsKey(clientId))
        {
            playerInitiativeOrder[clientId] = result;
        }
        else
        {
            playerInitiativeOrder.Add(clientId, result);
        }

        Debug.Log("playerInitiativeOrder count: " + playerInitiativeOrder.Count);

        foreach(var a in playerInitiativeOrder)
        {
            Debug.Log(a);
        }
    }

    //[ServerRpc(RequireOwnership = false)]
    //private void SetBattleResultServerRpc(int result, ServerRpcParams serverRpcParams = default)
    //{
    //    SetBattleResultClientRpc(result);
    //}

    //[ClientRpc]
    //private void SetBattleResultClientRpc(int result)
    //{

    //}
}
