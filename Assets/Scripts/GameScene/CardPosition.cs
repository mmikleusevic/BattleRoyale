﻿using System;
using UnityEngine;

[Serializable]
public class CardPosition
{
    [SerializeField] private Vector3 position;

    public Vector3 Position { get => position; }
    public Player Player { get; set; }
    public bool IsOccupied { get; set; }
}