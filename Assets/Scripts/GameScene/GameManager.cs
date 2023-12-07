using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;
using UnityEngine;

public class GameManager : StateMachine
{
    public static GameManager Instance { get; private set; }

    public event EventHandler OnToggleLocalGamePause;
    public event EventHandler OnMultiplayerGamePaused;
    public event EventHandler OnMultiplayerGameUnpaused;
    public event EventHandler OnLocalPlayerReadyChanged;
    
    [SerializeField] private Transform playerPrefab;
    [SerializeField] private List<Vector3> spawnPositionList = new List<Vector3>();

    private bool isLocalPlayerReady = false;
    private List<Player> players;
    private Dictionary<ulong, bool> playerReadyDictonary;
    private Dictionary<ulong, bool> playerPausedDictionary;
    private NetworkVariable<bool> isGamePaused = new NetworkVariable<bool>(false);

    private void Awake()
    {
        Instance = this;

        playerReadyDictonary = new Dictionary<ulong, bool>();
        playerPausedDictionary = new Dictionary<ulong, bool>();
    }

    public override void OnNetworkSpawn()
    {
        isGamePaused.OnValueChanged += IsGamePaused_OnValueChanged;

        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        }
    }

    public override void OnNetworkDespawn()
    {
        isGamePaused.OnValueChanged -= IsGamePaused_OnValueChanged;
    }

    public async void StartGame()
    {
        foreach (KeyValuePair<ulong, NetworkClient> client in NetworkManager.ConnectedClients)
        {
            Player player = client.Value.PlayerObject.GetComponent<Player>();

            players.Add(player);
        }

        await Awaitable.WaitForSecondsAsync(3f);

        SetState(new StartGame(this));
    }

    //TODO Fix

    //public async void OnStartButton()
    //{
    //    await state.Start();
    //}

    //public async void OnAttackCardButton()
    //{
    //    await state.AttackCard();
    //}

    //public async void OnMoveButton()
    //{
    //    await state.AttackCard();
    //}

    //public async void OnAttackPlayerButton()
    //{
    //    await state.AttackPlayer();
    //}

    //public async void OnEndTurnButton()
    //{
    //    await state.End();
    //}

    private void SceneManager_OnLoadEventCompleted(string sceneName, UnityEngine.SceneManagement.LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            int playerIndex = GameMultiplayer.Instance.GetPlayerDataIndexFromClientId(clientId);
            Vector3 position = spawnPositionList[playerIndex];
            
            Transform playerTransform = Instantiate(playerPrefab, position, playerPrefab.rotation, null);
            playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);

            GameMultiplayer.Instance.SetNameClientRpc(playerTransform.gameObject, "Player" + playerIndex);
        }

        isLocalPlayerReady = true;

        SetPlayerReadyServerRpc();

        OnLocalPlayerReadyChanged?.Invoke(this, EventArgs.Empty);
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
            StartGame();
        }
    }
}
