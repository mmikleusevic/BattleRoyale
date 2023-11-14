using UnityEngine;

public class Won : State
{
    public Won(BattleSystem battleSystem) : base(battleSystem)
    {

    }

    public override async Awaitable Start()
    {
        Debug.Log("You won the battle");

        await Awaitable.WaitForSecondsAsync(2f);
    }
}

