using UnityEngine;

public class BattleSystem : StateMachine
{
    public async void OnWaitingForPlayers()
    {
        await State.WaitingForPlayers();
    }

    public async void OnCountdownToStart()
    {
        SetState(new Begin(this));

        await Awaitable.WaitForSecondsAsync(3f);
    }

    public async void OnStartButton()
    {
        await State.Start();
    }

    public async void OnAttackCardButton()
    {
        await State.AttackCard();
    }

    public async void OnAttackPlayerButton()
    {
        await State.AttackPlayer();
    }

    public async void OnEndTurnButton()
    {
        await State.End();
    }
}
