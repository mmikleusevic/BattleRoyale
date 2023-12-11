using System.Collections;
using UnityEngine;

public partial class Begin
{
    public class PlayerTurn : BattleState
    {
        public PlayerTurn(GameManager battleSystem) : base(battleSystem)
        {
        }

        public override IEnumerator BeforeStart()
        {
            Debug.Log("Change Card if Needed");

            yield break;
        }

        public override IEnumerator Start()
        {
            Debug.Log("Choose an action");

            yield return new WaitForSeconds(1f);
        }

        public override IEnumerator AttackCard()
        {
            Debug.Log("Check if dead");
            Debug.Log("Check if drinking");

            battleSystem.SetState(new Won(battleSystem));
            battleSystem.SetState(new Lost(battleSystem));

            yield break;
        }

        public override IEnumerator AttackPlayer()
        {
            Debug.Log("Check if dead");
            Debug.Log("Check if drinking");
            Debug.Log("Check if won the card");
            Debug.Log("Choose card randomly");
            Debug.Log("Equip if all slots not filled");

            battleSystem.SetState(new Won(battleSystem));
            battleSystem.SetState(new Lost(battleSystem));

            yield break;
        }

        public override IEnumerator Move()
        {
            Debug.Log("Check if dead");
            Debug.Log("Move");

            battleSystem.SetState(new Move(battleSystem));

            yield break;
        }

        public override IEnumerator End()
        {
            Debug.Log("End turn");
            Debug.Log("Switch to other player");

            battleSystem.SetState(new EnemyTurn(battleSystem));

            yield return new WaitForSeconds(1f);
        }
    }
}

