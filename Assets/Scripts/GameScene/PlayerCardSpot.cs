using System;
using UnityEngine;

[Serializable]
public class PlayerCardSpot
{
    public Vector3 position;
    public Player Player { get; private set; }
    public bool IsOccupied { get; private set; }
}
