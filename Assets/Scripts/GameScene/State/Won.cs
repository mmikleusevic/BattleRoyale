using System.Threading.Tasks;
using UnityEngine;

public class Won : State
{
    public override async Task Start()
    {
        await base.Start();

        Debug.Log("You won the battle");
    }
}

