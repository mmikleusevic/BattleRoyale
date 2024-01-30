using System;
using System.Collections;
using UnityEngine;

public partial class GameFinished : State
{
    public static event EventHandler OnGameFinished;

    public override IEnumerator Start()
    {
        OnGameFinished?.Invoke(this, EventArgs.Empty);

        Debug.Log("Game finished");

        yield return new WaitForSeconds(2f);
    }
}

