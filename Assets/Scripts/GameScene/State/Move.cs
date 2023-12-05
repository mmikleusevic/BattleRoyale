using UnityEngine;

public class Move : State
{
    public Move(GameManager battleSystem) : base(battleSystem)
    {

    }

    public override async Awaitable Start()
    {
        Debug.Log("You moved.");

        await Awaitable.NextFrameAsync();
    }
}