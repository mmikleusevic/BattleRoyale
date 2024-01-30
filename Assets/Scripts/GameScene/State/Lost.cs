using System.Collections;
using UnityEngine;

public class Lost : State
{
    public override IEnumerator Start()
    {
        Debug.Log("You were defeated.");

        yield break;
    }
}