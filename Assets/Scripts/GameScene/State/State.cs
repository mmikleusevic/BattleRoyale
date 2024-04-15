using System;
using System.Threading.Tasks;
using UnityEngine;

public abstract class State : IDisposable
{
    public virtual async Task Start()
    {
        await Awaitable.WaitForSecondsAsync(1f);
    }

    public virtual async Task End()
    {
        GridManager.Instance.DisableCards();

        await Awaitable.WaitForSecondsAsync(1f);
    }

    public virtual void Dispose() { }
}
