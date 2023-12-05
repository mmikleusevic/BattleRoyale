using System;
using Unity.Netcode;
using UnityEngine;

public abstract class StateMachine : NetworkBehaviour
{
    protected State state;

    public static event EventHandler OnStateChanged;

    public async void SetState(State state)
    {
        await SetStateServerRpc(state);
    }

    [ServerRpc(RequireOwnership = false)]
    private async Awaitable SetStateServerRpc(State state)
    {
        await SetStateClientRpc(state);
    }

    [ClientRpc]
    private async Awaitable SetStateClientRpc(State state)
    {
        this.state = state;

        await this.state.Start();

        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }
}
