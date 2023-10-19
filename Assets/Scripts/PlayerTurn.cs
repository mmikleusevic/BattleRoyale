using UnityEngine;

public partial class Begin
{
    public class PlayerTurn : State
    {
        public PlayerTurn(BattleSystem battleSystem) : base(battleSystem)
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

            BattleSystem.SetState(new Won(BattleSystem));
            BattleSystem.SetState(new Lost(BattleSystem));

            await Awaitable.NextFrameAsync();
        }

        public override async Awaitable AttackPlayer()
        {
            Debug.Log("Check if dead");
            Debug.Log("Check if drinking");
            Debug.Log("Check if won the card");
            Debug.Log("Choose card randomly");
            Debug.Log("Equip if all slots not filled");

            BattleSystem.SetState(new Won(BattleSystem));
            BattleSystem.SetState(new Lost(BattleSystem));

            await Awaitable.NextFrameAsync();
        }

        public override async Awaitable End()
        {
            Debug.Log("End turn");
            Debug.Log("Switch to other player");

            BattleSystem.SetState(new EnemyTurn(BattleSystem));

            await Awaitable.WaitForSecondsAsync(1f);
        }
    }
}

