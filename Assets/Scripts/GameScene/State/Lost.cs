using System.Collections;
using UnityEngine;

public class Lost : BattleState
{
    public Lost(GameManager battleSystem) : base(battleSystem)
    {

    }

    public override IEnumerator Start()
    {
        Debug.Log("You were defeated.");

        yield break;
    }
}