using System;
using System.Threading.Tasks;
using UnityEngine;

public partial class GameFinished : State
{
    public static event EventHandler OnGameFinished;

    public override async Task Start()
    {
        await base.Start();

        OnGameFinished?.Invoke(this, EventArgs.Empty);

        Debug.Log("Game finished");
    }
}

