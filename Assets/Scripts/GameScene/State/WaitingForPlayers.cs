using System;
using System.Collections;
using UnityEngine;

public partial class WaitingForPlayers : State
{
    public event EventHandler OnWaitingForPlayers;

    public override IEnumerator Start()
    {
        OnWaitingForPlayers?.Invoke(this, EventArgs.Empty);

        Debug.Log("Waiting for players");

        yield return new WaitForSeconds(2f);
    }
}

