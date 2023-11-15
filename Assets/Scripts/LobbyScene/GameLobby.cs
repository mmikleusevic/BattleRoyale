using ParrelSync;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLobby : MonoBehaviour
{
    private const string KEY_RELAY_JOIN_CODE = "RelayJoinCode";
    public const string CREATOR_NAME = "CreatorName";
    public static GameLobby Instance { get; private set; }

    public event EventHandler OnReconnectStarted;
    public event EventHandler OnReconnectFailed;
    public event EventHandler OnCreateLobbyStarted;
    public event EventHandler OnCreateLobbyFailed;
    public event EventHandler OnJoinStarted;
    public event EventHandler OnQuickJoinFailed;
    public event EventHandler OnJoinFailed;
    public event EventHandler<OnLobbyListChangdEventArgs> OnLobbyListChanged;

    public class OnLobbyListChangdEventArgs : EventArgs
    {
        public List<Lobby> lobbyList;
    }

    private Lobby joinedLobby;
    private float heartbeatTimer;
    private float heartbeatTimerMax = 15f;
    private float listLobbiesTimer;
    private float listLobbiesTimerMax = 3f;
    private string lobbyName = "";

    private void Awake()
    {
        Instance = this;

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

    private void OnDestroy()
    {
        LobbyUI.Instance.OnLobbyFind -= LobbyUI_OnLobbySearch;
        CloseLobby();
    }

    private void LobbyUI_OnLobbySearch(object sender, LobbyUI.OnLobbyFindEventArgs e)
    {
        lobbyName = e.lobbyName;
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

    private async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions
            {
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT),
                    new QueryFilter(QueryFilter.FieldOptions.Name, lobbyName, QueryFilter.OpOptions.CONTAINS),
                }
            };

            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync(queryLobbiesOptions);

            OnLobbyListChanged?.Invoke(this, new OnLobbyListChangdEventArgs
            {
                lobbyList = queryResponse.Results
            });
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError(ex.Message);
        }
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

    private async Task<Allocation> AllocateRelay()
    {
        try
        {
            return await RelayService.Instance.CreateAllocationAsync(GameMultiplayer.MAX_PLAYER_AMOUNT - 1);
        }
        catch (RelayServiceException ex)
        {
            Debug.LogError(ex.Message);

            return default;
        }
    }

    private async Task<string> GetRelayJoinCode(Allocation allocation)
    {
        try
        {
            return await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        }
        catch (RelayServiceException ex)
        {
            Debug.LogError(ex.Message);

            return default;
        }
    }

    private async Task<JoinAllocation> JoinRelay(string joinCode)
    {
        try
        {
            return await RelayService.Instance.JoinAllocationAsync(joinCode);
        }
        catch (RelayServiceException ex)
        {
            Debug.LogError(ex.Message);

            return default;
        }
    }

    public async void CreateLobby(string lobbyName, bool isPrivate)
    {
        if (string.IsNullOrEmpty(lobbyName))
        {
            OnCreateLobbyFailed?.Invoke(this, EventArgs.Empty);
            return;
        }

        OnCreateLobbyStarted?.Invoke(this, EventArgs.Empty);

        try
        {
            joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, GameMultiplayer.MAX_PLAYER_AMOUNT, new CreateLobbyOptions
            {
                IsPrivate = isPrivate,
                Data = new Dictionary<string, DataObject>
                {
                    { CREATOR_NAME, new DataObject(DataObject.VisibilityOptions.Public, GameMultiplayer.Instance.GetPlayerName()) }
                }
            });

            Allocation allocation = await AllocateRelay();

            string relayJoinCode = await GetRelayJoinCode(allocation);

            await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { KEY_RELAY_JOIN_CODE, new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) }
                }
            });

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));

            GameMultiplayer.Instance.StartHost();

            LevelManager.Instance.LoadNetwork(Scene.CharacterScene);
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError(ex.Message);
            OnCreateLobbyFailed?.Invoke(this, EventArgs.Empty);
        }
    }

    public async void Reconnect(string lobbyId)
    {
        OnReconnectStarted?.Invoke(this, EventArgs.Empty);

        try
        {
            joinedLobby = await LobbyService.Instance.ReconnectToLobbyAsync(lobbyId);

            LobbyJoinRelayStartClient();
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError(ex.Message);
            OnReconnectFailed?.Invoke(this, EventArgs.Empty);
        }

    }

    public async void QuickJoin()
    {
        JoinStarted();

        try
        {
            joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();

            LobbyJoinRelayStartClient();
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError(ex.Message);
            OnQuickJoinFailed?.Invoke(this, EventArgs.Empty);
        }
    }

    public async void JoinWithId(string lobbyId)
    {
        if (string.IsNullOrEmpty(lobbyId)) return;

        JoinStarted();

        try
        {
            joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);

            LobbyJoinRelayStartClient();
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError(ex.Message);
            OnJoinFailed?.Invoke(this, EventArgs.Empty);
        }
    }

    public async void JoinWithCode(string lobbyCode)
    {
        if (string.IsNullOrEmpty(lobbyCode)) return;

        JoinStarted();

        try
        {
            joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);

            LobbyJoinRelayStartClient();
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError(ex.Message);
            OnJoinFailed?.Invoke(this, EventArgs.Empty);
        }
    }

    public async void DeleteLobby()
    {
        if (joinedLobby != null)
        {
            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);

                joinedLobby = null;
            }
            catch (LobbyServiceException ex)
            {
                Debug.LogError(ex.Message);
            }
        }
    }

    public async void LeaveLobby()
    {
        if (joinedLobby != null)
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, AuthenticationService.Instance.PlayerId);

                joinedLobby = null;
            }
            catch (LobbyServiceException ex)
            {
                Debug.LogError(ex.Message);
            }
        }
    }

    public async void KickPlayer(string playerId)
    {
        if (IsLobbyHost())
        {
            try
            {
                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);
            }
            catch (LobbyServiceException ex)
            {
                Debug.LogError(ex.Message);
            }
        }
    }

    private async void LobbyJoinRelayStartClient()
    {
        string relayJoinCode = joinedLobby.Data[KEY_RELAY_JOIN_CODE].Value;

        JoinAllocation joinAllocation = await JoinRelay(relayJoinCode);

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));

        GameMultiplayer.Instance.StartClient();
    }

    public void LeaveLobbyGoToMainMenu()
    {
        CloseLobby();

        LevelManager.Instance.LoadScene(Scene.MainMenuScene);
    }

    public void CloseLobby()
    {
        if (IsLobbyHost())
        {
            DeleteLobby();
            GameMultiplayer.Instance.StopHost();
        }
        else
        {
            LeaveLobby();
            GameMultiplayer.Instance.StopClient();
        }
    }

    private void JoinStarted()
    {
        OnJoinStarted?.Invoke(this, EventArgs.Empty);
    }

    public Lobby GetLobby()
    {
        return joinedLobby;
    }

    public bool LobbyExists()
    {
        return joinedLobby != null;
    }
}
