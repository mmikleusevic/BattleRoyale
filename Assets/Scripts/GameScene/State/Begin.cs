using UnityEngine;

public partial class Begin : State
{
    public Begin(GameManager battleSystem) : base(battleSystem)
    {
    }

    public override async Awaitable Start()
    {
        Debug.Log("Player turn started");

        battleSystem.SetState(new PlayerTurn(battleSystem));

        await Awaitable.WaitForSecondsAsync(3f);
    }
}

