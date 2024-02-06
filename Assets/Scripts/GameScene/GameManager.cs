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
    [SerializeField] private StateManager stateManager;

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
        isGamePaused.OnValueChanged += IsGamePaused_OnValueChanged;
        RollResults.OnInitiativeRollOver += RollResults_OnInitiativeRollOver;
        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
        PlayerManager.Instance.OnPlayersOrderSet += PlayerManager_OnPlayersOrderSet;
    }

    public override void OnNetworkDespawn()
    {
        isGamePaused.OnValueChanged -= IsGamePaused_OnValueChanged;
        RollResults.OnInitiativeRollOver -= RollResults_OnInitiativeRollOver;
        PlayerManager.Instance.OnPlayersOrderSet -= PlayerManager_OnPlayersOrderSet;

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

    private void RollResults_OnInitiativeRollOver(object sender, RollResults.OnInitiativeRollOverEventArgs e)
    {
        SetPlayerToPlayersList(e.playerOrder);
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong obj)
    {
        Time.timeScale = 1;

        GameLobby.Instance.DisconnectClientsOnServerLeaving(obj);
    }

    private void PlayerManager_OnPlayersOrderSet(object sender, EventArgs e)
    {
        SetStateForNextPlayer(StateEnum.PlaceOnGrid);
    }

    public void SetState(StateEnum state, ClientRpcParams clientRpcParams = default)
    {
        SetStateClientRpc(state, clientRpcParams);
    }

    [ClientRpc]
    private void SetStateClientRpc(StateEnum state, ClientRpcParams clientRpcParams = default)
    {
        stateManager.SetState(state);
    }

    [ServerRpc]
    private void StartGameServerRpc()
    {
        SpawnPlayers();

        SetState(StateEnum.Initiative);
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

    private void SetStateForNextPlayer(StateEnum state)
    {
        PlayerManager.Instance.SetNextActivePlayerClientRpc();

        Player activePlayer = PlayerManager.Instance.ActivePlayer;

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { activePlayer.ClientId.Value }
            }
        };

        GameManager.Instance.SetState(state, clientRpcParams);
    }

    [ServerRpc(RequireOwnership = false)]
    public void NextClientPlacingServerRpc()
    {
        if (PlayerManager.Instance.ActivePlayer == PlayerManager.Instance.LastPlayer)
        {
            SetStateForNextPlayer(StateEnum.PlayerTurn);
        }
        else
        {
            SetStateForNextPlayer(StateEnum.PlaceOnGrid);
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
