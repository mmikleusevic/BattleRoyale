using System;
using Unity.Netcode;
using UnityEngine;

public abstract class StateMachine : NetworkBehaviour
{
    protected State state;

    public static event EventHandler OnStateChanged;

    public void SetState(State state)
    {
        SetStateServerRpc(state.NetworkObject);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetStateServerRpc(NetworkObjectReference networkObject)
    {
        SetStateClientRpc(networkObject);
    }

    [ClientRpc]
    private void SetStateClientRpc(NetworkObjectReference networkObject, ClientRpcParams clientRpcParams = default)
    {
        networkObject.TryGet(out NetworkObject stateNetworkObject);

        if (stateNetworkObject == null) return;

        State state = stateNetworkObject.GetComponent<State>();

        this.state = state;

        this.state.Start();

        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }
}
