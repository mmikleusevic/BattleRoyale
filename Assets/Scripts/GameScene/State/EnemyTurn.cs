using System.Collections;
using UnityEngine;

public class EnemyTurn : BattleState
{
    public EnemyTurn(GameManager battleSystem) : base(battleSystem)
    {

    }

    public override IEnumerator Start()
    {
        Debug.Log("Disable most controls of the player");

        yield break;
    }
}

