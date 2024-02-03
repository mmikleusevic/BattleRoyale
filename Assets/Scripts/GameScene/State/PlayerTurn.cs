using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class PlayerTurn : State
{
    public override async Task Start()
    {
        await base.Start();

        Debug.Log("Choose an action");
    }

    public IEnumerator AttackCard()
    {
        Debug.Log("Check if dead");
        Debug.Log("Check if drinking");

        GameManager.Instance.SetState(StateEnum.Won);
        GameManager.Instance.SetState(StateEnum.Lost);

        yield break;
    }

    public IEnumerator AttackPlayer()
    {
        Debug.Log("Check if dead");
        Debug.Log("Check if drinking");
        Debug.Log("Check if won the card");
        Debug.Log("Choose card randomly");
        Debug.Log("Equip if all slots not filled");

        GameManager.Instance.SetState(StateEnum.Won);
        GameManager.Instance.SetState(StateEnum.Lost);

        yield break;
    }

    public IEnumerator Move()
    {
        Debug.Log("Check if dead");
        Debug.Log("Move");

        GameManager.Instance.SetState(StateEnum.Move);

        yield break;
    }

    public async override Task End()
    {
        Debug.Log("End turn");
        Debug.Log("Switch to other player");

        GameManager.Instance.SetState(StateEnum.EnemyTurn);

        await Awaitable.EndOfFrameAsync();
    }
}

