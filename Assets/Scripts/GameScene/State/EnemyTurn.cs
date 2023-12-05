using UnityEngine;

public class EnemyTurn : State
{
    public EnemyTurn(GameManager battleSystem) : base(battleSystem)
    {

    }

    public override async Awaitable Start()
    {
        Debug.Log("Disable most controls of the player");

        await Awaitable.NextFrameAsync();
    }
}

