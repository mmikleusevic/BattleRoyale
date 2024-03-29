using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;

public class PlayerManager : NetworkBehaviour
{
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

        NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_ServerOnClientDisconnectCallback;

        base.OnNetworkSpawn();
    }

    public override void OnNetworkDespawn()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback -= NetworkManager_ServerOnClientDisconnectCallback;
        }

        base.OnNetworkDespawn();
    }

    private void NetworkManager_ServerOnClientDisconnectCallback(ulong clientId)
    {
        if (NetworkManager.Singleton == null || NetworkManager.ServerClientId == clientId || NetworkManager.Singleton.ShutdownInProgress) return;

        Player player = Players.FirstOrDefault(a => a.ClientId.Value == clientId);

        if (player == null || player.NetworkObject == null) return;

        player.NetworkObject.DontDestroyWithOwner = true;

        if (ActivePlayer == player)
        {
            StateManager.Instance.GiveCurrentStateToSetNext(player.currentState);
        }

        player.DisablePlayer();

        RemovePlayerSetNewLastPlayerClientRpc(player.NetworkObject);

        MessageUI.Instance.SendMessageToEveryoneExceptMe(CreateOnPlayerLeftGameMessage(player));
    }

    [ClientRpc]
    private void RemovePlayerSetNewLastPlayerClientRpc(NetworkObjectReference playerNetworkObjectReference, ClientRpcParams clientRpcParams = default)
    {
        Player player = Player.GetPlayerFromNetworkReference(playerNetworkObjectReference);

        RemoveFromActivePlayers(player);
    }

    public void RemoveFromActivePlayers(Player player)
    {
        ActivePlayers.Remove(player);

        if (ActivePlayers.Count == 1)
        {
            GameManager.Instance.DetermineWinnerAndLosers();
        }

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

        PCInfoUI.Instance.SetActivePlayerText(ActivePlayer);

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
