using System.Collections;
using UnityEngine;

public partial class Begin : BattleState
{
    public Begin(GameManager battleSystem) : base(battleSystem)
    {
    }

    public override IEnumerator Start()
    {
        Debug.Log("Player turn started");

        yield return new WaitForSeconds(2f);

        battleSystem.SetState(new PlayerTurn(battleSystem));
    }
}

