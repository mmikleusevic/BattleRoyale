using System.Threading.Tasks;
using UnityEngine;

public class Move : State
{
    public override async Task Start()
    {
        await base.Start();

        Debug.Log("You moved.");
    }
}