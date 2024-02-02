using System.Threading.Tasks;
using UnityEngine;

public class Lost : State
{
    public override async Task Start()
    {
        await base.Start();

        Debug.Log("You were defeated.");
    }
}