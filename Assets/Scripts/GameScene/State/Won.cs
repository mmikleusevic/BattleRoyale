using System.Collections;
using UnityEngine;

public class Won : State
{
    public override IEnumerator Start()
    {
        Debug.Log("You won the battle");

        yield return new WaitForSeconds(2f);
    }
}

