using System;
using System.Collections;
using UnityEngine;

public partial class Initiative : State
{
    public static event EventHandler<string> OnInitiativeStart;

    public override IEnumerator Start()
    {
        string message = "ROLL FOR INITIATIVE";

        OnInitiativeStart?.Invoke(this, message);

        yield return new WaitForSeconds(2f);
    }
}

