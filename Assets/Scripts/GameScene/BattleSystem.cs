using UnityEngine;

public class BattleSystem : StateMachine
{
    public async void OnWaitingForPlayers()
    {
        await state.WaitingForPlayers();
    }

    public async void OnCountdownToStart()
    {
        SetState(new Begin(this));

        await Awaitable.WaitForSecondsAsync(3f);
    }

    public async void OnStartButton()
    {
        await state.Start();
    }

    public async void OnAttackCardButton()
    {
        await state.AttackCard();
    }

    public async void OnAttackPlayerButton()
    {
        await state.AttackPlayer();
    }

    public async void OnEndTurnButton()
    {
        await state.End();
    }
}
