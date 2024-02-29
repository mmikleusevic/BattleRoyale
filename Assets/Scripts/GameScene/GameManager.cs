using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public event EventHandler OnToggleLocalGamePause;
    public event EventHandler OnMultiplayerGamePaused;
    public event EventHandler OnMultiplayerGameUnpaused;

    [SerializeField] private Transform playerPrefab;
    [SerializeField] private List<Vector3> spawnPositionList;

    private bool autoCheckGamePauseState;

    private Dictionary<ulong, bool> playerReadyDictonary;
    private Dictionary<ulong, bool> playerPausedDictionary;
    private NetworkVariable<bool> isGamePaused;

    private void Awake()
    {
        Instance = this;

        playerReadyDictonary = new Dictionary<ulong, bool>();
        playerPausedDictionary = new Dictionary<ulong, bool>();
        isGamePaused = new NetworkVariable<bool>(false);
    }

    public override void OnNetworkSpawn()
    {
        StateManager.Instance.SetState(StateEnum.WaitingForPlayers);

        isGamePaused.OnValueChanged += IsGamePaused_OnValueChanged;
        InitiativeResults.OnInitiativeRollOver += InitiativeResults_OnInitiativeRollOver;
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
    }

    public override void OnNetworkDespawn()
    {
        isGamePaused.OnValueChanged -= IsGamePaused_OnValueChanged;
        InitiativeResults.OnInitiativeRollOver -= InitiativeResults_OnInitiativeRollOver;

        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= NetworkManager_OnClientDisconnectCallback;
        }

        base.OnNetworkDespawn();
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

    private void InitiativeResults_OnInitiativeRollOver(object sender, InitiativeResults.OnInitiativeRollOverEventArgs e)
    {
        SetPlayerToPlayersList(e.playerOrder);
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong obj)
    {
        Time.timeScale = 1;

        GameLobby.Instance.DisconnectClientsOnServerLeaving(obj);
    }

    [ServerRpc]
    private void StartGameServerRpc()
    {
        SpawnPlayers();

        StateManager.Instance.SetStateToClients(StateEnum.Initiative);
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
            playerNetworkObject.TrySetParent(PlayerManager.Instance.transform);
            GameMultiplayer.Instance.SetNameClientRpc(playerTransform.gameObject, "Player" + playerIndex);
        }
    }

    private void SetPlayerToPlayersList(List<ulong> playerOrder)
    {
        Player[] players = FindObjectsByType<Player>(FindObjectsSortMode.None);

        Player player = null;
        foreach (ulong clientId in playerOrder)
        {
            player = players.Where(a => a.ClientId.Value == clientId).FirstOrDefault();
            PlayerManager.Instance.Players.Add(player);
        }

        PlayerManager.Instance.SetPlayerReadyServerRpc(true);
    }
}
