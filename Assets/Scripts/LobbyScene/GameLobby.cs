using ParrelSync;
using System;
using System.Collections.Generic;
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
    public const string LOBBY_COLOR = "LobbyColor";

    public static GameLobby Instance { get; private set; }

    public event EventHandler OnCreateLobbyStarted;
    public event EventHandler OnCreateLobbyFailed;
    public event EventHandler OnJoinStarted;
    public event EventHandler OnQuickJoinFailed;
    public event EventHandler OnJoinFailed;
    public event EventHandler OnLobbyDeleted;
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
        OnLobbyDeleted += GameLobby_OnLobbyDeleted;
    }

    private void OnDisable()
    {
        LobbyUI.Instance.OnLobbyFind -= LobbyUI_OnLobbySearch;
        OnLobbyDeleted -= GameLobby_OnLobbyDeleted;
    }

    private void LobbyUI_OnLobbySearch(object sender, LobbyUI.OnLobbyFindEventArgs e)
    {
        lobbyName = e.lobbyName;
        ListLobbiesPeriodicallyAfterTimer();
    }

    private void GameLobby_OnLobbyDeleted(object sender, EventArgs e)
    {
        SetLobbyNullToEveryoneServerRpc();
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
                    new QueryFilter(QueryFilter.FieldOptions.Name, lobbyName, QueryFilter.OpOptions.CONTAINS)
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
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
                    { CREATOR_NAME, new DataObject(DataObject.VisibilityOptions.Public, GameMultiplayer.Instance.GetPlayerName()) },
                    { LOBBY_COLOR, new DataObject(DataObject.VisibilityOptions.Public, Color.black.ToString()) }
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

    public async void UpdateLobbyColor()
    {
        await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
        {
            Data = new Dictionary<string, DataObject>
            {
                { LOBBY_COLOR, new DataObject(DataObject.VisibilityOptions.Public, Color.red.ToString()) }
            }
        });
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

    public async Task DeleteLobby()
    {
        if (LobbyExists())
        {
            try
            {
                await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);

                OnLobbyDeleted?.Invoke(this, EventArgs.Empty);
            }
            catch (LobbyServiceException ex)
            {
                Debug.LogError(ex.Message);
            }
        }
    }

    public async Task LeaveLobby()
    {
        if (LobbyExists())
        {
            try
            {
                string playerId = AuthenticationService.Instance.PlayerId;

                await LobbyService.Instance.RemovePlayerAsync(joinedLobby.Id, playerId);

                SetLobbyToNull();
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

    public async void LeaveLobbyOrDelete()
    {
        if (IsLobbyHost())
        {
            GameMultiplayer.Instance.DisconnectClientsFromGame();
            await DeleteLobby();
        }
        else
        {
            await LeaveLobby();
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

    [ServerRpc]
    private void SetLobbyNullToEveryoneServerRpc()
    {
        SetLobbyNullToEveryoneClientRpc();
    }

    [ClientRpc]
    private void SetLobbyNullToEveryoneClientRpc()
    {
        SetLobbyToNull();
    }

    private void SetLobbyToNull()
    {
        joinedLobby = null;
    }
}
