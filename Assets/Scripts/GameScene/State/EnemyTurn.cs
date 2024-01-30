using System.Collections;
using UnityEngine;

public class EnemyTurn : State
{
    public override IEnumerator Start()
    {
        Debug.Log("Disable most controls of the player");

        yield break;
    }
}

