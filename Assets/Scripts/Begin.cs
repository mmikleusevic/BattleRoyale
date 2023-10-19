using UnityEngine;

public partial class Begin : State
{
    public Begin(BattleSystem battleSystem) : base(battleSystem)
    {
    }

    public override async Awaitable Start()
    {
        Debug.Log("Player turn started");

        BattleSystem.SetState(new PlayerTurn(BattleSystem));

        await Awaitable.WaitForSecondsAsync(2f);
    }
}

