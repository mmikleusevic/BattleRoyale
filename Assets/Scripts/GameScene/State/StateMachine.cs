using System;
using Unity.Netcode;

public abstract class StateMachine : NetworkBehaviour
{
    protected BattleState state;

    public static event EventHandler OnStateChanged;

    public void SetState(BattleState state)
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

        BattleState state = stateNetworkObject.GetComponent<BattleState>();

        this.state = state;

        this.state.Start();

        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }
}
