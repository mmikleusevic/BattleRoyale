using System.Threading.Tasks;
using UnityEngine;

public abstract class State
{
    public virtual async Task Start()
    {
        await Awaitable.WaitForSecondsAsync(1f);
    }

    public virtual async Task End()
    {
        GridManager.Instance.DisableCards();

        await Awaitable.WaitForSecondsAsync(.5f);
    }
}
