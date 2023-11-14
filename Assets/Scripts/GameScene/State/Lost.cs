using UnityEngine;

public class Lost : State
{
    public Lost(BattleSystem battleSystem) : base(battleSystem)
    {

    }

    public override async Awaitable Start()
    {
        Debug.Log("You were defeated.");

        await Awaitable.NextFrameAsync();
    }
}