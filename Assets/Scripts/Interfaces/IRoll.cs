using System.Collections;
using UnityEngine;

public interface IRoll
{
    IEnumerator RotateDice(GameObject[] dice, Vector3[] dicePositions, Vector3 cameraPosition);
}