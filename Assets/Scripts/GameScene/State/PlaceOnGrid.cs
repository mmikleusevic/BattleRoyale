using System.Collections;
using UnityEngine;

public class PlaceOnGrid : State
{
    public override IEnumerator Start()
    {
        Debug.Log("You placed yourself.");

        yield break;
    }
}