﻿using System.Threading.Tasks;
using UnityEngine;

public class EnemyTurn : State
{
    public override async Task Start()
    {
        await base.Start();

        Debug.Log("Disable most controls of the player");
    }
}

