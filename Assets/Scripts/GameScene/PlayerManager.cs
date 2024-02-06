using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class PlayerManager : NetworkBehaviour
{
    public static PlayerManager Instance { get; private set; }

    public event EventHandler OnPlayersOrderSet;

    public List<Player> Players { get; private set; }
    private Dictionary<ulong, bool> clientsReady;
    public Player ActivePlayer { get; private set; }
    public Player LastPlayer { get; private set; }

    private int activeIndex = -1;

    private void Awake()
    {
        Instance = this;

        Players = new List<Player>();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        PrepareClientDictionaryReady();

        base.OnNetworkSpawn();
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

        activeIndex = (activeIndex + 1) % Players.Count;

        ActivePlayer = Players[activeIndex];

        ActivePlayer.ShowParticleCircle();
    }

    [ClientRpc]
    public void SetLastPlayerClientRpc(NetworkObjectReference networkObjectReference)
    {
        networkObjectReference.TryGet(out NetworkObject networkObject);

        if (networkObject == null) return;

        Player lastPlayer = networkObject.GetComponent<Player>();

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

        OnPlayersOrderSet?.Invoke(this, EventArgs.Empty);
    }
}
