using ParrelSync;
using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLobby : NetworkBehaviour
{
    public const string KEY_RELAY_JOIN_CODE = "RelayJoinCode";
    public const string CREATOR_NAME = "CreatorName";
    public const string LOBBY_COLOR = "LobbyColor";

    public static GameLobby Instance { get; private set; }

    private RelayServiceHandler relayServiceHandler;
    private LobbyServiceHandler lobbyServiceHandler;
    private Lobby joinedLobby;

    private float heartbeatTimer;
    private float heartbeatTimerMax = 15f;
    private float listLobbiesTimer;
    private float listLobbiesTimerMax = 3f;
    private string lobbyName = "";
    private bool disconnected = false;

    private void Awake()
    {
        Instance = this;

        relayServiceHandler = new RelayServiceHandler();
        lobbyServiceHandler = new LobbyServiceHandler(relayServiceHandler);

        DontDestroyOnLoad(gameObject);

        InitializeUnityAuthentication();
    }

    private void Update()
    {
        HandleHeartbeat();
        HandlePeriodicListLobbies();
    }

    private void Start()
    {
        LobbyUI.Instance.OnLobbyFind += LobbyUI_OnLobbySearch;
    }

    private void OnDisable()
    {
        LobbyUI.Instance.OnLobbyFind -= LobbyUI_OnLobbySearch;
    }

    public override void OnDestroy()
    {
        lobbyServiceHandler.Dispose();
        lobbyServiceHandler = null;
        relayServiceHandler = null;

        base.OnDestroy();
    }

    private void LobbyUI_OnLobbySearch(object sender, LobbyUI.OnLobbyFindEventArgs e)
    {
        lobbyName = e.lobbyName.ToUpper();
        ListLobbiesPeriodicallyAfterTimer();
    }

    private void HandlePeriodicListLobbies()
    {
        if (joinedLobby == null &&
            AuthenticationService.Instance.IsSignedIn &&
            SceneManager.GetActiveScene().name == Scene.LobbyScene.ToString())
        {
            ListLobbiesPeriodicallyAfterTimer();
        }
    }

    private void HandleHeartbeat()
    {
        if (IsLobbyHost())
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer <= 0)
            {
                heartbeatTimer = heartbeatTimerMax;

                LobbyService.Instance.SendHeartbeatPingAsync(joinedLobby.Id);
            }
        }
    }

    public bool IsLobbyHost()
    {
        return joinedLobby != null && joinedLobby.HostId == AuthenticationService.Instance.PlayerId;
    }

    private void ListLobbiesPeriodicallyAfterTimer()
    {
        if (joinedLobby == null)
        {
            listLobbiesTimer -= Time.deltaTime;

            if (listLobbiesTimer <= 0)
            {
                listLobbiesTimer = listLobbiesTimerMax;
                ListLobbies();
            }
        }
    }

    private async void ListLobbies()
    {
        try
        {
            await lobbyServiceHandler.ListLobbies(lobbyName);
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    private async void InitializeUnityAuthentication()
    {
        if (UnityServices.State != ServicesInitializationState.Initialized)
        {
            InitializationOptions initializationOptions = new InitializationOptions();

#if UNITY_EDITOR
            //ParralelSync fix when joining from different editors on the same computer
            initializationOptions.SetProfile(ClonesManager.IsClone() ? ClonesManager.GetArgument() : "Primary");
#endif

            await UnityServices.InitializeAsync(initializationOptions);

            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
    }

    public async void CreateLobby(string lobbyName, bool isPrivate)
    {
        try
        {
            joinedLobby = await lobbyServiceHandler.CreateLobby(lobbyName, isPrivate);
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    public async void UpdateLobbyColor()
    {
        try
        {
            await lobbyServiceHandler.UpdateLobbyColor(joinedLobby);
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    public async void QuickJoin()
    {
        try
        {
            joinedLobby = await lobbyServiceHandler.QuickJoin();
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError(ex.Message);
            string playerId = AuthenticationService.Instance.PlayerId;
            await RemovePlayer(playerId);
        }
    }

    public async void JoinWithId(string lobbyId)
    {
        try
        {
            joinedLobby = await lobbyServiceHandler.JoinWithId(lobbyId);
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError(ex.Message);
            string playerId = AuthenticationService.Instance.PlayerId;
            await RemovePlayer(playerId);
        }
    }

    public async void JoinWithCode(string lobbyCode)
    {
        try
        {
            joinedLobby = await lobbyServiceHandler.JoinWithCode(lobbyCode);
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError(ex.Message);
            string playerId = AuthenticationService.Instance.PlayerId;
            await RemovePlayer(playerId);
        }
    }

    public async Task DeleteLobby()
    {
        try
        {
            if (!disconnected)
            {
                disconnected = true;

                await lobbyServiceHandler.DeleteLobby(joinedLobby);
            }
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    public async Task LeaveLobby()
    {
        try
        {
            if (!disconnected)
            {
                disconnected = true;

                string playerId = AuthenticationService.Instance.PlayerId;

                await lobbyServiceHandler.RemovePlayer(joinedLobby, playerId);
            }
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    public async Task RemovePlayer(string playerId)
    {
        try
        {
            await lobbyServiceHandler.RemovePlayer(joinedLobby, playerId);
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    public async Task LeaveLobbyOrDelete()
    {
        try
        {
            if (IsLobbyHost())
            {
                DisconnectClientsFromGame();
                await DeleteLobby();
            }
            else
            {
                NetworkManager.Singleton.Shutdown();

                await LeaveLobby();
            }
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    public void DisconnectClientsFromGame()
    {
        DisconnectClientsFromGameServerRpc();
    }

    [ServerRpc]
    public void DisconnectClientsFromGameServerRpc(ServerRpcParams serverRpcParams = default)
    {
        ulong[] clientIds = new ulong[NetworkManager.ConnectedClientsIds.Count];
        int i = 0;

        foreach (ulong clientId in NetworkManager.ConnectedClientsIds.Reverse())
        {
            clientIds[i] = clientId;

            i++;
        }

        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = clientIds
            }
        };

        DisconnectClientsFromGameClientRpc(clientRpcParams);
    }

    [ClientRpc]
    public void DisconnectClientsFromGameClientRpc(ClientRpcParams clientRpcParams = default)
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            SetLobbyToNull();
        }

        NetworkManager.Singleton.Shutdown();
    }

    public void SetLobbyToNull()
    {
        joinedLobby = null;
    }

    public Lobby GetLobby()
    {
        return joinedLobby;
    }

    public void DisconnectClient(ulong clientId)
    {
        NetworkManager.Singleton.DisconnectClient(clientId);
    }

    public async void DisconnectClientsOnServerLeaving(ulong clientId)
    {
        if (clientId == NetworkManager.ServerClientId && NetworkManager.Singleton.IsConnectedClient)
        {
            await Awaitable.NextFrameAsync();

            LevelManager.Instance.LoadScene(Scene.MainMenuScene);
        }
    }
}
