using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Unity.Services.Lobbies.Models;

public class PlayerManager : NetworkBehaviour
{
    public static event Action<string> OnPlayerLeftGame;
    public static PlayerManager Instance { get; private set; }

    public event Action<Player> OnActivePlayerChanged;

    private Dictionary<ulong, bool> clientsReady;

    public List<Player> Players { get; private set; }
    public List<Player> ActivePlayers { get; private set; }
    public Player ActivePlayer { get; private set; }
    public Player LastPlayer { get; private set; }

    private int activeIndex = -1;

    private void Awake()
    {
        Instance = this;

        Players = new List<Player>();
        ActivePlayers = new List<Player>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        PrepareClientDictionaryReady();

        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;

        base.OnNetworkSpawn();
    }

    public override void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= NetworkManager_OnClientDisconnectCallback;
        }

        base.OnDestroy();
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong obj)
    {
        Player player = Players.FirstOrDefault(a => a.ClientId.Value == obj);

        if (player == null || player.NetworkObject == null || NetworkManager.ShutdownInProgress || NetworkManager.ServerClientId == obj) return;

        if (ActivePlayer == player)
        {
            StateManager.Instance.GiveCurrentStateToSetNext(player.currentState);
        }

        OnPlayerLeftGame?.Invoke(CreateOnPlayerLeftGameMessage(player));

        RemovePlayerSetNewLastPlayerClientRpc(player.NetworkObject);
    }

    [ClientRpc]
    private void RemovePlayerSetNewLastPlayerClientRpc(NetworkObjectReference playerNetworkObjectReference, ClientRpcParams clientRpcParams = default)
    {
        Player player = Player.GetPlayerFromNetworkReference(playerNetworkObjectReference);
        player.DisablePlayer();

        RemoveFromActivePlayers(player);
    }

    public void RemoveFromActivePlayers(Player player)
    {
        ActivePlayers.Remove(player);

        Player lastPlayer = ActivePlayers.LastOrDefault();
        LastPlayer = lastPlayer;
    }

    private void PrepareClientDictionaryReady()
    {
        clientsReady = new Dictionary<ulong, bool>();

        foreach (var item in NetworkManager.ConnectedClientsIds)
        {
            clientsReady[item] = false;
        }
    }

    [ClientRpc]
    public void SetNextActivePlayerClientRpc()
    {
        if (ActivePlayer)
        {
            ActivePlayer.HideParticleCircle();
        }

        FindNextActiveIndexSetActivePlayer();        
    }

    private void FindNextActiveIndexSetActivePlayer()
    {
        int nextIndex = (activeIndex + 1) % ActivePlayers.Count;

        ActivePlayer = ActivePlayers[nextIndex];

        activeIndex = nextIndex;

        OnActivePlayerChanged?.Invoke(ActivePlayer);

        ActivePlayer.ShowParticleCircle();
    }

    [ClientRpc]
    public void SetLastPlayerClientRpc(NetworkObjectReference networkObjectReference)
    {
        Player lastPlayer = Player.GetPlayerFromNetworkReference(networkObjectReference);

        LastPlayer = lastPlayer;
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetPlayerReadyServerRpc(bool value, ServerRpcParams serverRpcParams = default)
    {
        clientsReady[serverRpcParams.Receive.SenderClientId] = value;

        foreach (var item in clientsReady)
        {
            if (item.Value == false)
            {
                return;
            }
        }

        Player lastPlayer = Players.LastOrDefault();

        SetLastPlayerClientRpc(lastPlayer.NetworkObject);

        StateManager.Instance.GiveCurrentStateToSetNext(StateEnum.Initiative);
    }

    private string CreateOnPlayerLeftGameMessage(Player player)
    {
        return $"<color=#{player.HexPlayerColor}>{player.PlayerName} </color>LEFT THE GAME";
    }
}
