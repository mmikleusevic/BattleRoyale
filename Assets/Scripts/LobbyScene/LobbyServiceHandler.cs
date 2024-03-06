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
    public static event EventHandler OnQuickJoinFailed;
    public static event EventHandler OnJoinFailed;
    public static event EventHandler OnCreateLobbyStarted;
    public static event EventHandler OnCreateLobbyFailed;
    public static event EventHandler OnJoinStarted;
    public static event EventHandler<OnLobbyListChangdEventArgs> OnLobbyListChanged;

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

            OnLobbyListChanged?.Invoke(this, new OnLobbyListChangdEventArgs
            {
                lobbyList = queryResponse.Results
            });
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError(ex.Message);
            throw;
        }
    }

    public async Task<Lobby> CreateLobby(string lobbyName, bool isPrivate)
    {
        if (string.IsNullOrEmpty(lobbyName))
        {
            OnCreateLobbyFailed?.Invoke(this, EventArgs.Empty);
            return null;
        }

        OnCreateLobbyStarted?.Invoke(this, EventArgs.Empty);

        try
        {
            Lobby joinedLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName.ToUpper(), GameMultiplayer.MAX_PLAYER_AMOUNT, new CreateLobbyOptions
            {
                IsPrivate = isPrivate,
                Data = new Dictionary<string, DataObject>
                {
                    { CREATOR_NAME, new DataObject(DataObject.VisibilityOptions.Public, GameMultiplayer.Instance.GetPlayerName()) },
                    { LOBBY_COLOR, new DataObject(DataObject.VisibilityOptions.Public, Color.black.ToString()) }
                }
            });

            Allocation allocation = await relayServiceHandler.AllocateRelay();

            string relayJoinCode = await relayServiceHandler.GetRelayJoinCode(allocation);

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

            return joinedLobby;
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError(ex.Message);
            OnCreateLobbyFailed?.Invoke(this, EventArgs.Empty);
            throw;
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
            throw;
        }
    }

    public async Task<Lobby> QuickJoin()
    {
        try
        {
            OnJoinStarted?.Invoke(this, EventArgs.Empty);

            Lobby joinedLobby = await LobbyService.Instance.QuickJoinLobbyAsync();

            await relayServiceHandler.LobbyJoinRelayStartClient(joinedLobby);

            return joinedLobby;
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError(ex.Message);

            OnQuickJoinFailed?.Invoke(this, EventArgs.Empty);
            await Instance.LeaveLobby();
            throw;
        }
    }

    public async Task<Lobby> JoinWithId(string lobbyId)
    {
        if (string.IsNullOrEmpty(lobbyId)) return null;

        try
        {
            OnJoinStarted?.Invoke(this, EventArgs.Empty);

            Lobby joinedLobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);

            await relayServiceHandler.LobbyJoinRelayStartClient(joinedLobby);

            return joinedLobby;
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError(ex.Message);
            OnJoinFailed?.Invoke(this, EventArgs.Empty);
            throw;
        }
    }

    public async Task<Lobby> JoinWithCode(string lobbyCode)
    {
        if (string.IsNullOrEmpty(lobbyCode)) return null;

        try
        {
            OnJoinStarted?.Invoke(this, EventArgs.Empty);

            Lobby joinedLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode);

            await relayServiceHandler.LobbyJoinRelayStartClient(joinedLobby);

            return joinedLobby;
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError(ex.Message);
            OnJoinFailed?.Invoke(this, EventArgs.Empty);
            throw;
        }
    }

    public async Task LeaveLobby(Lobby joinedLobby)
    {
        try
        {
            if (joinedLobby == null) return;

            string playerId = AuthenticationService.Instance.PlayerId;

            if(playerId == null) return;

            await LobbyService.Instance.RemovePlayerAsync(joinedLobby?.Id, playerId);
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    public async Task<bool> LobbyExists(Lobby joinedLobby)
    {
        try
        {
            if (joinedLobby != null)
            {
                joinedLobby = await LobbyService.Instance.GetLobbyAsync(joinedLobby?.Id);
            }

            return joinedLobby != null;
        }
        catch (LobbyServiceException ex)
        {
            Debug.LogError(ex.Message);
            return false;
        }
    }

    public async Task DeleteLobby(Lobby joinedLobby)
    {
        try
        {
            bool lobbyExists = await LobbyExists(joinedLobby);

            if (!lobbyExists) return;

            await LobbyService.Instance.DeleteLobbyAsync(joinedLobby.Id);
        }
        catch (LobbyServiceException ex)
        {
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