using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance { get; private set; }

    public event Action OnToggleLocalGamePause;
    public event Action OnMultiplayerGamePaused;
    public event Action OnMultiplayerGameUnpaused;

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

        base.OnNetworkSpawn();
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
            OnMultiplayerGamePaused?.Invoke();
        }
        else
        {
            Time.timeScale = 1f;
            OnMultiplayerGameUnpaused?.Invoke();
        }
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong obj)
    {
        Time.timeScale = 1;

        GameLobby.Instance.DisconnectClientsOnServerLeaving(obj);
    }

    private void InitiativeResults_OnInitiativeRollOver(InitiativeResults.OnInitiativeRollOverEventArgs e)
    {
        SetPlayerToPlayersList(e.playerOrder);
    }

    [ServerRpc]
    private void StartGameServerRpc()
    {
        SpawnPlayers();

        StateManager.Instance.GiveCurrentStateToSetNext(StateEnum.WaitingForPlayers);
    }

    public void TogglePauseGame()
    {
        TogglePauseGameServerRpc();

        OnToggleLocalGamePause?.Invoke();
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
            PlayerManager.Instance.ActivePlayers.Add(player);
        }

        PlayerManager.Instance.SetPlayerReadyServerRpc(true);
    }

    public void DisablePlayersOnLastCard()
    {
        List<Player> playersOrderedByMostCards = PlayerManager.Instance.ActivePlayers.OrderByDescending(a => a.EquippedCards.Count + a.UnequippedCards.Count).ToList();

        Player playerWithMostCards = playersOrderedByMostCards.FirstOrDefault();
        int cardCount = playerWithMostCards.UnequippedCards.Count + playerWithMostCards.EquippedCards.Count;

        foreach (Player player in playersOrderedByMostCards)
        {
            if (player == playerWithMostCards) continue;

            int playerCardCount = player.EquippedCards.Count + player.UnequippedCards.Count;

            if ((cardCount - playerCardCount) > 3)
            {
                player.DisablePlayer();

                MessageUI.Instance.SendMessageToEveryoneExceptMe(CreateOnPlayerLostGameMessage(player));
            }
        }

        MessageUI.Instance.SendMessageToEveryoneExceptMe(CreateOnLastCardLeftGameMessage());
    }

    public void DetermineWinnerAndLosers()
    {
        Player winner = PlayerManager.Instance.ActivePlayers.OrderByDescending(a => a.Points.Value).FirstOrDefault();

        ClientRpcParams clientRpcParamsWinner = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { winner.ClientId.Value }
            }
        };

        ulong[] loserIds = PlayerManager.Instance.ActivePlayers.FindAll(a => a.ClientId.Value != winner.ClientId.Value).Select(a => a.ClientId.Value).ToArray();


        if (loserIds.Length > 0)
        {
            ClientRpcParams clientRpcParamsLosers = new ClientRpcParams
            {
                Send = new ClientRpcSendParams
                {
                    TargetClientIds = loserIds
                }
            };

            StateManager.Instance.SetStateToClients(StateEnum.Lost, clientRpcParamsLosers);
        }

        StateManager.Instance.SetStateToClients(StateEnum.Won, clientRpcParamsWinner);
    }

    private string CreateOnPlayerLostGameMessage(Player player)
    {
        return $"<color=#{player.HexPlayerColor}>{player.PlayerName} </color>HAD MORE THAN A 3-CARD DIFFERENCE COMPARED TO THE FIRST PLAYER, AND THUS HAS LOST";
    }

    public string CreateOnLastCardLeftGameMessage()
    {
        return $"KEEP MOVING TOWARDS ADJACENT FIELDS AROUND LAST CARD";
    }
}
