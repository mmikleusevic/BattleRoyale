using System;
using System.Threading.Tasks;
using UnityEngine;

public class Won : State
{
    public static event Action<string> OnWon;

    public override async Task Start()
    {
        await base.Start();

        OnWon?.Invoke(CreateOnWonMessage());
    }

    private string CreateOnWonMessage()
    {
        return "YOU WON!";
    }
}

