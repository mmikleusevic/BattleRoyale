using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
            LevelManager.Instance.LoadNetwork(Scene.GameScene);
            Task.FromResult(GameLobby.Instance.DeleteLobby());
        }
    }

    [ClientRpc]
    private void TogglePlayerReadyClientRpc(ulong clientId)
    {
        if (playerReadyDictonary.ContainsKey(clientId))
        {
            playerReadyDictonary[clientId] = !playerReadyDictonary[clientId];
        }
        else
        {
            playerReadyDictonary[clientId] = true;
        }

        OnReadyChanged.Invoke(this, EventArgs.Empty);
    }

    public bool IsPlayerReady(ulong clientId)
    {
        return playerReadyDictonary.ContainsKey(clientId) && playerReadyDictonary[clientId];
    }
}
