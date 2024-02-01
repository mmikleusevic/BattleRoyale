using System;
using System.Collections;
using UnityEngine;

public partial class WaitingForPlayers : State
{
    public static event EventHandler<string> OnWaitingForPlayers;

    public override IEnumerator Start()
    {
        string message = "WAITING FOR PLAYERS";

        OnWaitingForPlayers?.Invoke(this, message);

        yield return new WaitForSeconds(3f);
    }
}

