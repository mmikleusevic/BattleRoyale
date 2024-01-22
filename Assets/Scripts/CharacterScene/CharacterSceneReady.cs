using System;
using System.Collections.Generic;
using Unity.Netcode;

public class CharacterSceneReady : NetworkBehaviour
{
    public static CharacterSceneReady Instance { get; private set; }

    public event EventHandler OnReadyChanged;

    private Dictionary<ulong, bool> playerReadyDictonary;

    private void Awake()
    {
        Instance = this;

        playerReadyDictonary = new Dictionary<ulong, bool>();
    }

    public void TogglePlayerReady()
    {
        TogglePlayerReadyServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void TogglePlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        TogglePlayerReadyClientRpc(serverRpcParams.Receive.SenderClientId);

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!playerReadyDictonary.ContainsKey(clientId) || !playerReadyDictonary[clientId])
            {
                return;
            }
        }

        LevelManager.Instance.LoadNetwork(Scene.GameScene);
        GameLobby.Instance.UpdateLobbyColor();

    }

    [ClientRpc]
    private void TogglePlayerReadyClientRpc(ulong key)
    {
        if (playerReadyDictonary.ContainsKey(key))
        {
            playerReadyDictonary[key] = !playerReadyDictonary[key];
        }
        else
        {
            playerReadyDictonary[key] = true;
        }

        OnReadyChanged.Invoke(this, EventArgs.Empty);
    }

    public bool IsPlayerReady(ulong clientId)
    {
        return playerReadyDictonary.ContainsKey(clientId) && playerReadyDictonary[clientId];
    }

    public void GetPlayerReadyValuesForClient()
    {
        GetPlayerReadyValuesForClientServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void GetPlayerReadyValuesForClientServerRpc(ServerRpcParams serverRpcParams = default)
    {
        ClientRpcParams clientRpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new[] { serverRpcParams.Receive.SenderClientId }
            }
        };

        foreach (KeyValuePair<ulong, bool> playerReady in playerReadyDictonary)
        {
            GetPlayerReadyValuesForClientClientRpc(playerReady.Key, playerReady.Value, clientRpcParams);
        }
    }

    [ClientRpc]
    private void GetPlayerReadyValuesForClientClientRpc(ulong key, bool value, ClientRpcParams clientRpcParams = default)
    {
        playerReadyDictonary[key] = value;
    }

    public void RemoveKeyFromPlayerReady()
    {
        RemoveKeyFromPlayerReadyServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void RemoveKeyFromPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        SetServerNotReadyServerRpc();
        RemoveKeyFromPlayerReadyClientRpc(serverRpcParams.Receive.SenderClientId);
    }

    [ClientRpc]
    private void RemoveKeyFromPlayerReadyClientRpc(ulong key, ClientRpcParams clientRpcParams = default)
    {
        playerReadyDictonary.Remove(key);
    }

    [ServerRpc]
    private void SetServerNotReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        SetServerNotReadyClientRpc();

        OnReadyChanged?.Invoke(this, EventArgs.Empty);
    }

    [ClientRpc]
    private void SetServerNotReadyClientRpc(ClientRpcParams clientRpcParams = default)
    {
        playerReadyDictonary[NetworkManager.ServerClientId] = false;
    }
}
