using System;
using System.Collections;
using UnityEngine;

public class PlaceOnGrid : State
{
    public override IEnumerator Start()
    {
        Debug.Log("You have to place yourself.");

        GridManager.Instance.PlacePlayerOnGrid();

        yield break;
    }
}