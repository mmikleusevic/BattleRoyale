using System;
using System.Collections;
using UnityEngine;

public partial class Initiative : State
{
    public static event EventHandler OnInitiativeStart;

    public override IEnumerator Start()
    {
        OnInitiativeStart?.Invoke(this, EventArgs.Empty);

        Debug.Log("Initiative");

        yield return new WaitForSeconds(2f);
    }
}

