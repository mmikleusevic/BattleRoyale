using System.Collections;
using UnityEngine;

public class Move : State
{
    public override IEnumerator Start()
    {
        Debug.Log("You moved.");

        yield break;
    }
}