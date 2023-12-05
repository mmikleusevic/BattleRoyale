using UnityEngine;

public partial class Begin
{
    public class PlayerTurn : State
    {
        public PlayerTurn(GameManager battleSystem) : base(battleSystem)
        {

        }

        public override async Awaitable BeforeStart()
        {
            Debug.Log("Change Card if Needed");

            await Awaitable.NextFrameAsync();
        }

        public override async Awaitable Start()
        {
            Debug.Log("Choose an action");

            await Awaitable.WaitForSecondsAsync(1f);
        }

        public override async Awaitable AttackCard()
        {
            Debug.Log("Check if dead");
            Debug.Log("Check if drinking");

            battleSystem.SetState(new Won(battleSystem));
            battleSystem.SetState(new Lost(battleSystem));

            await Awaitable.NextFrameAsync();
        }

        public override async Awaitable AttackPlayer()
        {
            Debug.Log("Check if dead");
            Debug.Log("Check if drinking");
            Debug.Log("Check if won the card");
            Debug.Log("Choose card randomly");
            Debug.Log("Equip if all slots not filled");

            battleSystem.SetState(new Won(battleSystem));
            battleSystem.SetState(new Lost(battleSystem));

            await Awaitable.NextFrameAsync();
        }

        public override async Awaitable Move()
        {
            Debug.Log("Check if dead");
            Debug.Log("Move");

            battleSystem.SetState(new Move(battleSystem));

            await Awaitable.NextFrameAsync();
        }

        public override async Awaitable End()
        {
            Debug.Log("End turn");
            Debug.Log("Switch to other player");

            battleSystem.SetState(new EnemyTurn(battleSystem));

            await Awaitable.WaitForSecondsAsync(1f);
        }
    }
}

