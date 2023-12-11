using System.Collections;
using UnityEngine;

public class Move : BattleState
{
    public Move(GameManager battleSystem) : base(battleSystem)
    {
    }

    public override IEnumerator Start()
    {
        Debug.Log("You moved.");

        yield break;
    }
}