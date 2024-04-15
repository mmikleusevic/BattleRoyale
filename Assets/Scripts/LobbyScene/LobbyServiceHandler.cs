using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay.Models;
using UnityEngine;
using static GameLobby;

public class LobbyServiceHandler : IDisposable
{
    public static event Action OnQuickJoinFailed;
    public static event Action OnJoinFailed;
    public static event Action OnCreateLobbyStarted;
    public static event Action OnCreateLobbyFailed;
    public static event Action OnJoinStarted;
    public static event Action OnRemovingJoinedLobbies;
    public static event Action OnRemovingJoinedLobbiesFailed;
    public static event Action OnRemovingJoinedLobbiesOver;
    public static event Action<OnLobbyListChangdEventArgs> OnLobbyListChanged;

    public class OnLobbyListChangdEventArgs : EventArgs
    {
        public List<Lobby> lobbyList;
    }

    private RelayServiceHandler relayServiceHandler;

    private bool disposed = false;

    public LobbyServiceHandler(RelayServiceHandler relayServiceHandler)
    {
        this.relayServiceHandler = relayServiceHandler;
    }

    public async Task ListLobbies(string lobbyName)
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

            OnLobbyListChanged?.Invoke(new OnLobbyListChangdEventArgs
            {
                lobbyList = queryResponse.Results
            });
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError(ex.Message);
            return;
        }
    }

    public async Task<Lobby> CreateLobby(string lobbyName, bool isPrivate)
    {
        if (string.IsNullOrEmpty(lobbyName))
        {
            OnCreateLobbyFailed?.Invoke();
            return null;
        }

        try
        {
            OnCreateLobbyStarted?.Invoke();

            Allocation allocation = await relayServiceHandler.AllocateRelay();

            string relayJoinCode = await relayServiceHandler.GetRelayJoinCode(allocation);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));

            GameMultiplayer.Instance.StartHost();

            Lobby joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName.ToUpper(), GameMultiplayer.MAX_PLAYER_AMOUNT, new CreateLobbyOptions
            {
                IsPrivate = isPrivate,
                Data = new Dictionary<string, DataObject>
                {
                    { CREATOR_NAME, new DataObject(DataObject.VisibilityOptions.Public, GameMultiplayer.Instance.GetPlayerName()) },
                    { LOBBY_COLOR, new DataObject(DataObject.VisibilityOptions.Public, Color.black.ToString()) }
                }
            });

            await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { KEY_RELAY_JOIN_CODE, new DataObject(DataObject.VisibilityOptions.Member, relayJoinCode) }
                }
            });

            LevelManager.Instance.LoadNetwork(Scene.CharacterScene);

            return joinedLobby;
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError(ex.Message);
            OnCreateLobbyFailed?.Invoke();
            return null;
        }
    }

    public async Task UpdateLobbyColor(Lobby joinedLobby)
    {
        try
        {
            await LobbyService.Instance.UpdateLobbyAsync(joinedLobby.Id, new UpdateLobbyOptions
            {
                Data = new Dictionary<string, DataObject>
                {
                    { LOBBY_COLOR, new DataObject(DataObject.VisibilityOptions.Public, Color.red.ToString()) }
                }
            });
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError(ex.Message);
            return;
        }
    }

    public async Task<Lobby> QuickJoin()
    {
        try
        {
            OnJoinStarted?.Invoke();

            Lobby joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();

            await relayServiceHandler.LobbyJoinRelayStartClient(joinedLobby);

            return joinedLobby;
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError(ex.Message);
            OnQuickJoinFailed?.Invoke();
            await ClearJoinedLobbiesOnJoinFailed();
            return null;
        }
    }

    public async Task<Lobby> JoinWithId(string lobbyId)
    {
        if (string.IsNullOrEmpty(lobbyId))
        {
            OnJoinFailed?.Invoke();
            return null;
        }

        try
        {
            OnJoinStarted?.Invoke();

            Lobby joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);

            await relayServiceHandler.LobbyJoinRelayStartClient(joinedLobby);

            return joinedLobby;
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError(ex.Message);
            OnJoinFailed?.Invoke();
            await ClearJoinedLobbiesOnJoinFailed();
            return null;
        }
    }

    public async Task<Lobby> JoinWithCode(string lobbyCode)
    {
        if (string.IsNullOrEmpty(lobbyCode))
        {
            OnJoinFailed?.Invoke();
            return null;
        }

        try
        {
            OnJoinStarted?.Invoke();

            Lobby joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);

            await relayServiceHandler.LobbyJoinRelayStartClient(joinedLobby);

            return joinedLobby;
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError(ex.Message);
            OnJoinFailed?.Invoke();
            await ClearJoinedLobbiesOnJoinFailed();
            return null;
        }
    }

    public async Task RemovePlayer(string lobbyId)
    {
        try
        {
            if (lobbyId == null) return;

            string playerId = AuthenticationService.Instance.PlayerId;

            if (playerId == null) return;

            await LobbyService.Instance.RemovePlayerAsync(lobbyId, playerId);
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    public async Task RemovePlayer(string lobbyId, string playerId)
    {
        try
        {
            if (lobbyId == null || playerId == null) return;

            await LobbyService.Instance.RemovePlayerAsync(lobbyId, playerId);
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    public async Task DeleteLobby(Lobby joinedLobby)
    {
        try
        {
            if (joinedLobby == null) return;

            await LobbyService.Instance.DeleteLobbyAsync(joinedLobby?.Id);
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    public async Task ClearJoinedLobbiesOnJoinFailed()
    {
        try
        {
            OnRemovingJoinedLobbies?.Invoke();

            List<string> lobbyIds = await LobbyService.Instance.GetJoinedLobbiesAsync();

            if (lobbyIds == null || lobbyIds.Count == 0) return;

            foreach (var item in lobbyIds)
            {
                await RemovePlayer(item);
            }

            OnRemovingJoinedLobbiesOver?.Invoke();
        }
        catch (LobbyServiceException ex)
        {
            OnRemovingJoinedLobbiesFailed?.Invoke();
            Debug.LogError(ex.Message);
        }
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {

                OnQuickJoinFailed = null;
                OnJoinFailed = null;
                OnCreateLobbyStarted = null;
                OnCreateLobbyFailed = null;
                OnJoinStarted = null;
                OnRemovingJoinedLobbies = null;
                OnRemovingJoinedLobbiesFailed = null;
                OnRemovingJoinedLobbiesOver = null;
                OnLobbyListChanged = null;
            }

            relayServiceHandler = null;

            disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
    }

    ~LobbyServiceHandler()
    {
        Dispose(false);
    }
}