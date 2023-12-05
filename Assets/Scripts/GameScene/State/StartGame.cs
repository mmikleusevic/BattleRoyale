using UnityEngine;

public partial class StartGame : State
{
    public StartGame(GameManager battleSystem) : base(battleSystem)
    {
    }

    public override async Awaitable Start()
    {
        Debug.Log("Game started");

        await base.Start();
    }
}

