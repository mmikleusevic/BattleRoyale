using System;
using System.Threading.Tasks;

public class EnemyTurn : State
{
    public static event Action OnEnemyTurn;

    public override async Task Start()
    {
        OnEnemyTurn?.Invoke();

        await base.Start();
    }
}

