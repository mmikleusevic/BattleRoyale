using System.Collections;
using Unity.Netcode;

public abstract class BattleState : NetworkBehaviour
{
    protected GameManager battleSystem;

    public BattleState(GameManager battleSystem)
    {
        this.battleSystem = battleSystem;
    }
    public virtual IEnumerator BeforeStart()
    {
        yield break;
    }

    public virtual IEnumerator Start()
    {
        yield break;
    }

    public virtual IEnumerator Move()
    {
        yield break;
    }

    public virtual IEnumerator AttackCard()
    {
        yield break;
    }

    public virtual IEnumerator AttackPlayer()
    {
        yield break;
    }

    public virtual IEnumerator Won()
    {
        yield break;
    }

    public virtual IEnumerator Lost()
    {
        yield break;
    }

    public virtual IEnumerator End()
    {
        yield break;
    }
}
