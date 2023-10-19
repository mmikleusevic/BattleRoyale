using UnityEngine;

public abstract class State
{
    protected BattleSystem BattleSystem;

    public State(BattleSystem battleSystem)
    {
        BattleSystem = battleSystem;
    }

    public virtual Awaitable WaitingForPlayers()
    {
        return Awaitable.NextFrameAsync();
    }

    public virtual Awaitable CountdownToStart()
    {
        return Awaitable.NextFrameAsync();
    }

    public virtual Awaitable BeforeStart()
    {
        return Awaitable.NextFrameAsync();
    }

    public virtual Awaitable Start()
    {
        return Awaitable.NextFrameAsync();
    }

    public virtual Awaitable AttackCard()
    {
        return Awaitable.NextFrameAsync();
    }

    public virtual Awaitable AttackPlayer()
    {
        return Awaitable.NextFrameAsync();
    }

    public virtual Awaitable Won()
    {
        return Awaitable.NextFrameAsync();
    }

    public virtual Awaitable End()
    {
        return Awaitable.NextFrameAsync();
    }
}
